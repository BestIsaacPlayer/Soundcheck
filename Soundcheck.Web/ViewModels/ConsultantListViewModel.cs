using Soundcheck.Web.Models;

namespace Soundcheck.Web.ViewModels;

public class ConsultantListViewModel
{
    public IReadOnlyList<ConsultantProfile> Consultants { get; init; } = new List<ConsultantProfile>();

    public IReadOnlyDictionary<int, decimal> AverageRatings { get; init; } = new Dictionary<int, decimal>();

    public IReadOnlyList<EquipmentCategory> Categories { get; init; } = new List<EquipmentCategory>();

    public int? SelectedCategoryId { get; init; }

    public string? SearchText { get; init; }
}
