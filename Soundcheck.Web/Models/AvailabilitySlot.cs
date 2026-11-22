namespace Soundcheck.Web.Models;

public class AvailabilitySlot
{
    public int Id { get; init; }

    public int ConsultantProfileId { get; init; }

    public ConsultantProfile ConsultantProfile { get; init; } = null!;

    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }

    public bool IsBooked { get; init; }

    public Booking? Booking { get; init; }
}
