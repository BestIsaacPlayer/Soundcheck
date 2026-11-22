using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.Models;

public class ConsultantProfile
{
    public int Id { get; init; }

    [Required]
    public string UserId { get; init; } = string.Empty;

    public ApplicationUser User { get; init; } = null!;

    [StringLength(1000)]
    public string? Bio { get; set; }

    [Range(0, 10000)]
    public decimal HourlyRate { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<AvailabilitySlot> AvailabilitySlots { get; init; } = new List<AvailabilitySlot>();

    public ICollection<ConsultantCategory> ConsultantCategories { get; init; } = new List<ConsultantCategory>();
}
