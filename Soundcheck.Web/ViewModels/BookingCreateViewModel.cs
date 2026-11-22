using Soundcheck.Web.Models;

namespace Soundcheck.Web.ViewModels;

public class BookingCreateViewModel
{
    public BookingCreateInputModel Input { get; set; } = new BookingCreateInputModel();

    public string ConsultantName { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public IReadOnlyList<EquipmentCategory> Categories { get; set; } = new List<EquipmentCategory>();
}
