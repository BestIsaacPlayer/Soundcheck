using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Soundcheck.Web.Data;
using Soundcheck.Web.Models;
using Soundcheck.Web.Services;

namespace Soundcheck.Tests;

public class BookingServiceTests
{
    [Fact]
    public async Task ConfirmBookingAsync_ByAssignedConsultant_ChangesStatusToConfirmed()
    {
        await using var databaseContext = TestDatabase.CreateContext();
        var booking = CreateBooking(databaseContext, BookingStatus.Pending);
        var bookingService = CreateService(databaseContext);

        var result = await bookingService.ConfirmBookingAsync(booking.Id, GetConsultantUserId(booking));

        Assert.True(result.Succeeded);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public async Task ConfirmBookingAsync_ByAnotherConsultant_FailsAndKeepsStatus()
    {
        await using var databaseContext = TestDatabase.CreateContext();
        var booking = CreateBooking(databaseContext, BookingStatus.Pending);
        var bookingService = CreateService(databaseContext);

        var result = await bookingService.ConfirmBookingAsync(booking.Id, "another-consultant-id");

        Assert.False(result.Succeeded);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public async Task ConfirmBookingAsync_ForUnknownBooking_Fails()
    {
        await using var databaseContext = TestDatabase.CreateContext();
        var bookingService = CreateService(databaseContext);

        var result = await bookingService.ConfirmBookingAsync(9999, "any-consultant-id");

        Assert.False(result.Succeeded);
        Assert.Equal("The booking does not exist.", result.ErrorMessage);
    }

    [Fact]
    public async Task CompleteBookingAsync_ForPendingBooking_FailsAndKeepsStatus()
    {
        await using var databaseContext = TestDatabase.CreateContext();
        var booking = CreateBooking(databaseContext, BookingStatus.Pending);
        var bookingService = CreateService(databaseContext);

        var result = await bookingService.CompleteBookingAsync(booking.Id, GetConsultantUserId(booking));

        Assert.False(result.Succeeded);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public async Task CompleteBookingAsync_ForConfirmedBooking_ChangesStatusToCompleted()
    {
        await using var databaseContext = TestDatabase.CreateContext();
        var booking = CreateBooking(databaseContext, BookingStatus.Confirmed);
        var bookingService = CreateService(databaseContext);

        var result = await bookingService.CompleteBookingAsync(booking.Id, GetConsultantUserId(booking));

        Assert.True(result.Succeeded);
        Assert.Equal(BookingStatus.Completed, booking.Status);
    }

    [Fact]
    public async Task AddReviewAsync_ForCompletedBooking_SavesReview()
    {
        await using var databaseContext = TestDatabase.CreateContext();
        var booking = CreateBooking(databaseContext, BookingStatus.Completed);
        var bookingService = CreateService(databaseContext);

        var result = await bookingService.AddReviewAsync(booking.Id, booking.ClientId, 5, "Very helpful");

        Assert.True(result.Succeeded);
        var review = await databaseContext.ConsultationReviews.SingleAsync();
        Assert.Equal(5, review.Rating);
        Assert.Equal("Very helpful", review.Comment);
    }

    [Fact]
    public async Task AddReviewAsync_ForPendingBooking_FailsAndSavesNothing()
    {
        await using var databaseContext = TestDatabase.CreateContext();
        var booking = CreateBooking(databaseContext, BookingStatus.Pending);
        var bookingService = CreateService(databaseContext);

        var result = await bookingService.AddReviewAsync(booking.Id, booking.ClientId, 4, null);

        Assert.False(result.Succeeded);
        Assert.Empty(databaseContext.ConsultationReviews);
    }

    [Fact]
    public async Task AddReviewAsync_CalledTwice_RejectsSecondReview()
    {
        await using var databaseContext = TestDatabase.CreateContext();
        var booking = CreateBooking(databaseContext, BookingStatus.Completed);
        var bookingService = CreateService(databaseContext);
        await bookingService.AddReviewAsync(booking.Id, booking.ClientId, 5, null);

        var secondResult = await bookingService.AddReviewAsync(booking.Id, booking.ClientId, 1, null);

        Assert.False(secondResult.Succeeded);
        var review = await databaseContext.ConsultationReviews.SingleAsync();
        Assert.Equal(5, review.Rating);
    }

    [Fact]
    public async Task AddReviewAsync_ByDifferentClient_Fails()
    {
        await using var databaseContext = TestDatabase.CreateContext();
        var booking = CreateBooking(databaseContext, BookingStatus.Completed);
        var bookingService = CreateService(databaseContext);

        var result = await bookingService.AddReviewAsync(booking.Id, "another-client-id", 5, null);

        Assert.False(result.Succeeded);
        Assert.Empty(databaseContext.ConsultationReviews);
    }

    private static BookingService CreateService(ApplicationDbContext databaseContext)
    {
        return new BookingService(databaseContext, NullLogger<BookingService>.Instance);
    }

    private static string GetConsultantUserId(Booking booking)
    {
        return booking.AvailabilitySlot.ConsultantProfile.UserId;
    }

    private static Booking CreateBooking(ApplicationDbContext databaseContext, BookingStatus status)
    {
        var consultantUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "consultant",
            FullName = "Anna Nowak"
        };

        var clientUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "client",
            FullName = "Jan Kowalski"
        };

        var consultantProfile = new ConsultantProfile
        {
            User = consultantUser,
            HourlyRate = 100m
        };

        var availabilitySlot = new AvailabilitySlot
        {
            ConsultantProfile = consultantProfile,
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(1).AddHours(1),
            IsBooked = true
        };

        var booking = new Booking
        {
            Client = clientUser,
            AvailabilitySlot = availabilitySlot,
            EquipmentCategory = new EquipmentCategory { Name = "Guitars" },
            Topic = "First guitar",
            Status = status,
            Price = 100m
        };

        databaseContext.Bookings.Add(booking);
        databaseContext.SaveChanges();

        return booking;
    }
}
