using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.Models;

public class ConsultantProfile
{
    public int Id { get; init; }

    [Required]
    public string UserId { get; init; } = string.Empty;

    public ApplicationUser User { get; init; } = null!;

    [StringLength(1000)]
    public string? Bio { get; init; }

    [Range(0, 10000)]
    public decimal HourlyRate { get; init; }

    public bool IsActive { get; init; } = true;

    public ICollection<AvailabilitySlot> AvailabilitySlots { get; init; } = new List<AvailabilitySlot>();
}
