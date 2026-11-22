using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.Models;

public class EquipmentCategory
{
    public int Id { get; init; }

    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }

    public ICollection<ConsultantCategory> ConsultantCategories { get; init; } = new List<ConsultantCategory>();

    public ICollection<Booking> Bookings { get; init; } = new List<Booking>();
}
