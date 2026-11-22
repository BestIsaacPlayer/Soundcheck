using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Controllers;
using Soundcheck.Web.Models;

namespace Soundcheck.Tests;

public class EquipmentCategoriesControllerTests
{
    [Fact]
    public async Task Create_WithValidCategory_SavesCategoryAndRedirectsToIndex()
    {
        await using var databaseContext = TestDatabase.CreateContext();
        var controller = new EquipmentCategoriesController(databaseContext);

        var result = await controller.Create(new EquipmentCategory { Name = "Drums" });

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(EquipmentCategoriesController.Index), redirectResult.ActionName);
        Assert.Equal(1, await databaseContext.EquipmentCategories.CountAsync());
    }
}
