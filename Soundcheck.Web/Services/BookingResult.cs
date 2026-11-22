namespace Soundcheck.Web.Services;

public class BookingResult
{
    public bool Succeeded { get; private init; }

    public int? BookingId { get; private init; }

    public string? ErrorMessage { get; private init; }

    public static BookingResult Success(int? bookingId = null)
    {
        return new BookingResult { Succeeded = true, BookingId = bookingId };
    }

    public static BookingResult Failure(string errorMessage)
    {
        return new BookingResult { Succeeded = false, ErrorMessage = errorMessage };
    }
}
