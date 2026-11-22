namespace Soundcheck.Web.Models;

public class AvailabilitySlot
{
    public int Id { get; init; }

    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }

    public bool IsBooked { get; init; }
}
