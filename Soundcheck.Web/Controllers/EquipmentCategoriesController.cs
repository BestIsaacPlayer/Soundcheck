using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;
using Soundcheck.Web.Models;

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

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description")] EquipmentCategory category)
    {
        if (await databaseContext.EquipmentCategories.AnyAsync(existingCategory => existingCategory.Name == category.Name))
        {
            ModelState.AddModelError(nameof(EquipmentCategory.Name), "A category with this name already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        databaseContext.EquipmentCategories.Add(category);
        await databaseContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
