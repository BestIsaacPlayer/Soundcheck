using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.ViewModels;

public class ConsultantCategoryInputModel
{
    public int EquipmentCategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public bool IsSelected { get; set; }

    [Range(0, 60)]
    public int YearsOfExperience { get; set; }
}
