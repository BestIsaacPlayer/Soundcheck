using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.Models;

public class ConsultantProfile
{
    public int Id { get; init; }

    [StringLength(1000)]
    public string? Bio { get; init; }

    [Range(0, 10000)]
    public decimal HourlyRate { get; init; }

    public bool IsActive { get; init; } = true;
}
