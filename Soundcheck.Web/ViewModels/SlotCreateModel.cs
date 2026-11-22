using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.ViewModels;

public class SlotCreateModel
{
    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }
}
