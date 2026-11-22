using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;
using Soundcheck.Web.Models;

namespace Soundcheck.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ConsultantManagementController : Controller
{
    private const string ConsultantRoleName = "Consultant";

    private readonly ApplicationDbContext databaseContext;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ILogger<ConsultantManagementController> logger;

    public ConsultantManagementController(
        ApplicationDbContext databaseContext,
        UserManager<ApplicationUser> userManager,
        ILogger<ConsultantManagementController> logger)
    {
        this.databaseContext = databaseContext;
        this.userManager = userManager;
        this.logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var users = await databaseContext.Users
            .AsNoTracking()
            .Include(user => user.ConsultantProfile)
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Email)
            .ToListAsync();

        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeConsultant(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var alreadyHasProfile = await databaseContext.ConsultantProfiles.AnyAsync(profile => profile.UserId == id);
        if (alreadyHasProfile)
        {
            TempData["ErrorMessage"] = "This user is already a consultant.";
            return RedirectToAction(nameof(Index));
        }

        if (!await userManager.IsInRoleAsync(user, ConsultantRoleName))
        {
            var roleResult = await userManager.AddToRoleAsync(user, ConsultantRoleName);
            if (!roleResult.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(" ", roleResult.Errors.Select(error => error.Description));
                return RedirectToAction(nameof(Index));
            }
        }

        databaseContext.ConsultantProfiles.Add(new ConsultantProfile
        {
            UserId = id,
            HourlyRate = 0,
            IsActive = false
        });

        await databaseContext.SaveChangesAsync();
        logger.LogInformation("User {UserId} was made a consultant", id);

        var displayName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
        TempData["SuccessMessage"] = $"{displayName} is now a consultant. The profile must be completed and activated before it appears on the list.";

        return RedirectToAction(nameof(Index));
    }
}
