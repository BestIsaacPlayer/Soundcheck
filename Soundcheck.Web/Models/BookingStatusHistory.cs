namespace Soundcheck.Web.Models;

public class BookingStatusHistory
{
    public int Id { get; init; }

    public BookingStatus OldStatus { get; init; }

    public BookingStatus NewStatus { get; init; }

    public DateTime ChangedAt { get; init; }
}
