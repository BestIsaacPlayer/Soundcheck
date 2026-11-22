namespace Soundcheck.Web.Services;

public interface IBookingService
{
    Task<BookingResult> BookConsultationAsync(string clientId, int availabilitySlotId, int equipmentCategoryId, string topic);

    Task<BookingResult> CancelBookingAsync(int bookingId);

    Task<BookingResult> ConfirmBookingAsync(int bookingId, string consultantUserId);

    Task<BookingResult> CompleteBookingAsync(int bookingId, string consultantUserId);

    Task<BookingResult> AddReviewAsync(int bookingId, string clientId, int rating, string? comment);
}
