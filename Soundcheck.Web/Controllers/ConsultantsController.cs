using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;
using Soundcheck.Web.ViewModels;

namespace Soundcheck.Web.Controllers;

public class ConsultantsController(ApplicationDbContext databaseContext) : Controller
{
    public async Task<IActionResult> Index(int? categoryId, string? searchText)
    {
        var consultantsQuery = databaseContext.ConsultantProfiles
            .AsNoTracking()
            .Include(profile => profile.User)
            .Include(profile => profile.ConsultantCategories)
                .ThenInclude(consultantCategory => consultantCategory.EquipmentCategory)
            .Where(profile => profile.IsActive);

        if (categoryId.HasValue)
        {
            consultantsQuery = consultantsQuery.Where(profile =>
                profile.ConsultantCategories.Any(consultantCategory => consultantCategory.EquipmentCategoryId == categoryId.Value));
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var trimmedSearchText = searchText.Trim();
            consultantsQuery = consultantsQuery.Where(profile => profile.User.FullName.Contains(trimmedSearchText));
        }

        var consultants = await consultantsQuery.OrderBy(profile => profile.User.FullName).ToListAsync();

        var viewModel = new ConsultantListViewModel
        {
            Consultants = consultants,
            Categories = await databaseContext.EquipmentCategories.AsNoTracking().OrderBy(category => category.Name).ToListAsync(),
            SelectedCategoryId = categoryId,
            SearchText = searchText
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var currentTime = DateTime.Now;

        var consultant = await databaseContext.ConsultantProfiles
            .AsNoTracking()
            .Include(profile => profile.User)
            .Include(profile => profile.ConsultantCategories)
                .ThenInclude(consultantCategory => consultantCategory.EquipmentCategory)
            .Include(profile => profile.AvailabilitySlots
                .Where(slot => !slot.IsBooked && slot.StartTime > currentTime)
                .OrderBy(slot => slot.StartTime))
            .FirstOrDefaultAsync(profile => profile.Id == id && profile.IsActive);

        if (consultant == null)
        {
            return NotFound();
        }

        return View(consultant);
    }
}
