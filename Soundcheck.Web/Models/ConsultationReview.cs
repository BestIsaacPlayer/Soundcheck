using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.Models;

public class ConsultationReview
{
    public int Id { get; init; }

    [Range(1, 5)]
    public int Rating { get; init; }

    [StringLength(1000)]
    public string? Comment { get; init; }
}
