using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.Models;

public class EquipmentCategory
{
    public int Id { get; init; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<ConsultantCategory> ConsultantCategories { get; init; } = new List<ConsultantCategory>();

    public ICollection<Booking> Bookings { get; init; } = new List<Booking>();
}
