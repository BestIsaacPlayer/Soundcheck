using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;

namespace Soundcheck.Web.Controllers;

public class EquipmentCategoriesController(ApplicationDbContext databaseContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var categories = await databaseContext.EquipmentCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync();

        return View(categories);
    }
}
