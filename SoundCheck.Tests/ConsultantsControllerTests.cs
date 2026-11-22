using Microsoft.AspNetCore.Mvc;
using Soundcheck.Web.Controllers;
using Soundcheck.Web.Data;
using Soundcheck.Web.Models;
using Soundcheck.Web.ViewModels;

namespace Soundcheck.Tests;

public class ConsultantsControllerTests
{
    [Fact]
    public async Task Index_WithoutFilters_ReturnsOnlyActiveConsultants()
    {
        await using var databaseContext = TestDatabase.CreateContext();
        AddConsultant(databaseContext, "Anna Nowak", true);
        AddConsultant(databaseContext, "Inactive Consultant", false);
        var controller = CreateController(databaseContext);

        var result = await controller.Index(null, null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ConsultantListViewModel>(viewResult.Model);
        var consultant = Assert.Single(viewModel.Consultants);
        Assert.Equal("Anna Nowak", consultant.User.FullName);
    }

    [Fact]
    public async Task Index_WithCategoryFilter_ReturnsOnlyConsultantsFromThatCategory()
    {
        await using var databaseContext = TestDatabase.CreateContext();
        var guitars = new EquipmentCategory { Name = "Guitars" };
        var keyboards = new EquipmentCategory { Name = "Keyboards" };
        databaseContext.EquipmentCategories.AddRange(guitars, keyboards);
        await databaseContext.SaveChangesAsync();
        AddConsultant(databaseContext, "Anna Nowak", true, guitars);
        AddConsultant(databaseContext, "Tomasz Wrobel", true, keyboards);
        var controller = CreateController(databaseContext);

        var result = await controller.Index(guitars.Id, null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ConsultantListViewModel>(viewResult.Model);
        var consultant = Assert.Single(viewModel.Consultants);
        Assert.Equal("Anna Nowak", consultant.User.FullName);
    }

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
