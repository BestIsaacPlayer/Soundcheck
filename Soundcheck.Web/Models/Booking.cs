using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.Models;

public class Booking
{
    public int Id { get; init; }

    [Required]
    public string ClientId { get; init; } = string.Empty;

    public ApplicationUser Client { get; init; } = null!;

    public int AvailabilitySlotId { get; init; }

    public AvailabilitySlot AvailabilitySlot { get; init; } = null!;

    public int EquipmentCategoryId { get; init; }

    public EquipmentCategory EquipmentCategory { get; init; } = null!;

    [Required]
    [StringLength(200)]
    public string Topic { get; init; } = string.Empty;

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [Range(0, 100000)]
    public decimal Price { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ConsultationReview? Review { get; init; }

    public ICollection<BookingStatusHistory> StatusHistory { get; init; } = new List<BookingStatusHistory>();
}
