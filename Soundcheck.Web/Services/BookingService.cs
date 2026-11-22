using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;
using Soundcheck.Web.Models;

namespace Soundcheck.Web.Services;

public class BookingService : IBookingService
{
    private static readonly int[] BusinessRuleErrorNumbers = { 50001, 50002, 50003, 50010, 50011 };

    private readonly ApplicationDbContext databaseContext;
    private readonly ILogger<BookingService> logger;

    public BookingService(ApplicationDbContext databaseContext, ILogger<BookingService> logger)
    {
        this.databaseContext = databaseContext;
        this.logger = logger;
    }

    public async Task<BookingResult> BookConsultationAsync(string clientId, int availabilitySlotId, int equipmentCategoryId, string topic)
    {
        var bookingIdParameter = new SqlParameter("@BookingId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        try
        {
            await databaseContext.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_BookConsultation @ClientId, @AvailabilitySlotId, @EquipmentCategoryId, @Topic, @BookingId OUTPUT",
                new SqlParameter("@ClientId", clientId),
                new SqlParameter("@AvailabilitySlotId", availabilitySlotId),
                new SqlParameter("@EquipmentCategoryId", equipmentCategoryId),
                new SqlParameter("@Topic", topic),
                bookingIdParameter);

            var bookingId = (int)bookingIdParameter.Value;
            logger.LogInformation("Booking {BookingId} created for client {ClientId} on slot {AvailabilitySlotId}", bookingId, clientId, availabilitySlotId);

            return BookingResult.Success(bookingId);
        }
        catch (SqlException exception) when (BusinessRuleErrorNumbers.Contains(exception.Number))
        {
            logger.LogWarning("Booking rejected for client {ClientId} on slot {AvailabilitySlotId}: {Reason}", clientId, availabilitySlotId, exception.Message);

            return BookingResult.Failure(exception.Message);
        }
    }

    public async Task<BookingResult> CancelBookingAsync(int bookingId)
    {
        try
        {
            await databaseContext.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_CancelBooking @BookingId",
                new SqlParameter("@BookingId", bookingId));

            logger.LogInformation("Booking {BookingId} cancelled", bookingId);

            return BookingResult.Success(bookingId);
        }
        catch (SqlException exception) when (BusinessRuleErrorNumbers.Contains(exception.Number))
        {
            logger.LogWarning("Cancelling booking {BookingId} rejected: {Reason}", bookingId, exception.Message);

            return BookingResult.Failure(exception.Message);
        }
    }

    public Task<BookingResult> ConfirmBookingAsync(int bookingId, string consultantUserId)
    {
        return ChangeStatusAsync(bookingId, consultantUserId, BookingStatus.Pending, BookingStatus.Confirmed);
    }

    public Task<BookingResult> CompleteBookingAsync(int bookingId, string consultantUserId)
    {
        return ChangeStatusAsync(bookingId, consultantUserId, BookingStatus.Confirmed, BookingStatus.Completed);
    }

    public async Task<BookingResult> AddReviewAsync(int bookingId, string clientId, int rating, string? comment)
    {
        var booking = await databaseContext.Bookings
            .Include(bookingWithReview => bookingWithReview.Review)
            .FirstOrDefaultAsync(candidate => candidate.Id == bookingId);

        if (booking == null)
        {
            return BookingResult.Failure("The booking does not exist.");
        }

        if (booking.ClientId != clientId)
        {
            return BookingResult.Failure("You can only review your own consultations.");
        }

        if (booking.Status != BookingStatus.Completed)
        {
            return BookingResult.Failure("Only completed consultations can be reviewed.");
        }

        if (booking.Review != null)
        {
            return BookingResult.Failure("This consultation has already been reviewed.");
        }

        databaseContext.ConsultationReviews.Add(new ConsultationReview
        {
            BookingId = bookingId,
            Rating = rating,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
        });

        await databaseContext.SaveChangesAsync();
        logger.LogInformation("Review with rating {Rating} added to booking {BookingId}", rating, bookingId);

        return BookingResult.Success(bookingId);
    }

    private async Task<BookingResult> ChangeStatusAsync(int bookingId, string consultantUserId, BookingStatus requiredStatus, BookingStatus newStatus)
    {
        var booking = await databaseContext.Bookings
            .Include(bookingWithSlot => bookingWithSlot.AvailabilitySlot)
                .ThenInclude(slot => slot.ConsultantProfile)
            .FirstOrDefaultAsync(candidate => candidate.Id == bookingId);

        if (booking == null)
        {
            return BookingResult.Failure("The booking does not exist.");
        }

        if (booking.AvailabilitySlot.ConsultantProfile.UserId != consultantUserId)
        {
            return BookingResult.Failure("You can only manage bookings assigned to you.");
        }

        if (booking.Status != requiredStatus)
        {
            return BookingResult.Failure($"Only {requiredStatus} bookings can be changed to {newStatus}.");
        }

        booking.Status = newStatus;
        await databaseContext.SaveChangesAsync();
        logger.LogInformation("Booking {BookingId} changed from {OldStatus} to {NewStatus} by consultant {ConsultantUserId}", bookingId, requiredStatus, newStatus, consultantUserId);

        return BookingResult.Success(bookingId);
    }
}
