namespace Soundcheck.Web.Models;

public class ConsultantRevenueReportRow
{
    public int ConsultantProfileId { get; set; }

    public string ConsultantName { get; set; } = string.Empty;

    public int EquipmentCategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int CompletedConsultations { get; set; }

    public decimal TotalRevenue { get; set; }
}
