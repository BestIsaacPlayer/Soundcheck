using Soundcheck.Web.Controllers;
using Soundcheck.Web.Data;
using Soundcheck.Web.Models;

namespace Soundcheck.Tests;

public class ConsultantsControllerTests
{
    private static ConsultantsController CreateController(ApplicationDbContext databaseContext)
    {
        return new ConsultantsController(databaseContext);
    }

    private static ConsultantProfile AddConsultant(
        ApplicationDbContext databaseContext,
        string fullName,
        bool isActive,
        params EquipmentCategory[] categories)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = fullName,
            FullName = fullName
        };

        var profile = new ConsultantProfile
        {
            User = user,
            HourlyRate = 100m,
            IsActive = isActive
        };

        foreach (var category in categories)
        {
            profile.ConsultantCategories.Add(new ConsultantCategory
            {
                EquipmentCategory = category,
                YearsOfExperience = 5
            });
        }

        databaseContext.ConsultantProfiles.Add(profile);
        databaseContext.SaveChanges();

        return profile;
    }
}
