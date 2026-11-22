using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.ViewModels;

public class ConsultantProfileEditModel
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Bio { get; set; }

    [Range(0, 10000)]
    public decimal HourlyRate { get; set; }

    public bool IsActive { get; set; }

    public List<ConsultantCategoryInputModel> Categories { get; set; } = new List<ConsultantCategoryInputModel>();
}
