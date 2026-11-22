using Soundcheck.Web.Models;

namespace Soundcheck.Web.ViewModels;

public class ConsultantListViewModel
{
    public IReadOnlyList<ConsultantProfile> Consultants { get; init; } = new List<ConsultantProfile>();

    public IReadOnlyList<EquipmentCategory> Categories { get; init; } = new List<EquipmentCategory>();

    public int? SelectedCategoryId { get; init; }

    public string? SearchText { get; init; }
}
