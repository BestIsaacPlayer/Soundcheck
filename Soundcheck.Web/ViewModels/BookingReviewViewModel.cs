namespace Soundcheck.Web.ViewModels;

public class BookingReviewViewModel
{
    public BookingReviewInputModel Input { get; set; } = new BookingReviewInputModel();

    public string ConsultantName { get; set; } = string.Empty;

    public string Topic { get; set; } = string.Empty;
}
