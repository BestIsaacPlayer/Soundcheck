using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.ViewModels;

public class BookingReviewInputModel
{
    public int BookingId { get; set; }

    [Range(1, 5, ErrorMessage = "Select a rating from 1 to 5.")]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }
}
