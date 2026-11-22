using System.ComponentModel.DataAnnotations;

namespace Soundcheck.Web.Models;

public class ConsultantCategory
{
    [Range(0, 60)]
    public int YearsOfExperience { get; init; }
}
