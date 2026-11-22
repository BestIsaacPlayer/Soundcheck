using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;
using Soundcheck.Web.Models;
using Soundcheck.Web.ViewModels;

namespace Soundcheck.Web.Controllers;

[Authorize(Roles = "Consultant")]
public class ConsultantProfileController(
    ApplicationDbContext databaseContext,
    ILogger<ConsultantProfileController> logger)
    : Controller
{
    public async Task<IActionResult> Edit()
    {
        var profile = await LoadCurrentProfileAsync();
        if (profile == null)
        {
            return NotFound();
        }

        var categories = await databaseContext.EquipmentCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync();

        var editModel = new ConsultantProfileEditModel
        {
            FullName = profile.User.FullName,
            Bio = profile.Bio,
            HourlyRate = profile.HourlyRate,
            IsActive = profile.IsActive,
            Categories = categories.Select(category =>
            {
                var existingConsultantCategory = profile.ConsultantCategories
                    .FirstOrDefault(consultantCategory => consultantCategory.EquipmentCategoryId == category.Id);

                return new ConsultantCategoryInputModel
                {
                    EquipmentCategoryId = category.Id,
                    CategoryName = category.Name,
                    IsSelected = existingConsultantCategory != null,
                    YearsOfExperience = existingConsultantCategory?.YearsOfExperience ?? 0
                };
            }).ToList()
        };

        return View(editModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ConsultantProfileEditModel input)
    {
        var profile = await LoadCurrentProfileAsync();
        if (profile == null)
        {
            return NotFound();
        }

        var hasSelectedCategory = input.Categories.Any(category => category.IsSelected);

        if (input.IsActive && input.HourlyRate <= 0)
        {
            ModelState.AddModelError(nameof(ConsultantProfileEditModel.HourlyRate),
                "Set an hourly rate greater than zero before activating your profile.");
        }

        if (input.IsActive && !hasSelectedCategory)
        {
            ModelState.AddModelError(string.Empty,
                "Select at least one specialization before activating your profile.");
        }

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        profile.User.FullName = input.FullName.Trim();
        profile.Bio = string.IsNullOrWhiteSpace(input.Bio) ? null : input.Bio.Trim();
        profile.HourlyRate = input.HourlyRate;
        profile.IsActive = input.IsActive;

        var knownCategoryIds = await databaseContext.EquipmentCategories.Select(category => category.Id).ToListAsync();

        foreach (var categoryInput in input.Categories.Where(category =>
                     knownCategoryIds.Contains(category.EquipmentCategoryId)))
        {
            var existingConsultantCategory = profile.ConsultantCategories
                .FirstOrDefault(consultantCategory =>
                    consultantCategory.EquipmentCategoryId == categoryInput.EquipmentCategoryId);

            if (categoryInput.IsSelected && existingConsultantCategory == null)
            {
                profile.ConsultantCategories.Add(new ConsultantCategory
                {
                    EquipmentCategoryId = categoryInput.EquipmentCategoryId,
                    YearsOfExperience = categoryInput.YearsOfExperience
                });
            }
            else if (categoryInput.IsSelected && existingConsultantCategory != null)
            {
                existingConsultantCategory.YearsOfExperience = categoryInput.YearsOfExperience;
            }
            else if (!categoryInput.IsSelected && existingConsultantCategory != null)
            {
                databaseContext.ConsultantCategories.Remove(existingConsultantCategory);
            }
        }

        await databaseContext.SaveChangesAsync();
        logger.LogInformation("Consultant profile {ConsultantProfileId} updated", profile.Id);

        TempData["SuccessMessage"] = "Your profile has been saved.";
        return RedirectToAction(nameof(Edit));
    }

    private Task<ConsultantProfile?> LoadCurrentProfileAsync()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return databaseContext.ConsultantProfiles
            .Include(profile => profile.User)
            .Include(profile => profile.ConsultantCategories)
            .FirstOrDefaultAsync(profile => profile.UserId == currentUserId);
    }
}