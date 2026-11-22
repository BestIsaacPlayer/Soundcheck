using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.Models;

public class ConsultantCategory
{
    public int ConsultantProfileId { get; init; }

    public ConsultantProfile ConsultantProfile { get; init; } = null!;

    [Range(0, 60)]
    public int YearsOfExperience { get; init; }
}
