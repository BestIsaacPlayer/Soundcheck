using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.ViewModels;

public class BookingCreateInputModel
{
    public int AvailabilitySlotId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select an equipment category.")]
    public int EquipmentCategoryId { get; set; }

    [Required]
    [StringLength(200)]
    public string Topic { get; set; } = string.Empty;
}
