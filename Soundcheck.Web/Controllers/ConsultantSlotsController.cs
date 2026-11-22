using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;
using Soundcheck.Web.Models;
using Soundcheck.Web.ViewModels;

namespace Soundcheck.Web.Controllers;

[Authorize(Roles = "Consultant")]
public class ConsultantSlotsController(ApplicationDbContext databaseContext, ILogger<ConsultantSlotsController> logger)
    : Controller
{
    private static readonly TimeSpan MaximumSlotDuration = TimeSpan.FromHours(4);

    public async Task<IActionResult> Index()
    {
        var profile = await FindCurrentProfileAsync();
        if (profile == null)
        {
            return NotFound();
        }

        var currentTime = DateTime.Now;

        var slots = await databaseContext.AvailabilitySlots
            .AsNoTracking()
            .Where(slot => slot.ConsultantProfileId == profile.Id && slot.StartTime > currentTime)
            .OrderBy(slot => slot.StartTime)
            .ToListAsync();

        return View(slots);
    }

    public IActionResult Create()
    {
        var firstProposedStart = DateTime.Today.AddDays(1).AddHours(10);

        return View(new SlotCreateModel
        {
            StartTime = firstProposedStart,
            EndTime = firstProposedStart.AddHours(1)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SlotCreateModel input)
    {
        var profile = await FindCurrentProfileAsync();
        if (profile == null)
        {
            return NotFound();
        }

        if (input.StartTime <= DateTime.Now)
        {
            ModelState.AddModelError(nameof(SlotCreateModel.StartTime), "The start time must be in the future.");
        }

        if (input.EndTime <= input.StartTime)
        {
            ModelState.AddModelError(nameof(SlotCreateModel.EndTime), "The end time must be after the start time.");
        }
        else if (input.EndTime - input.StartTime > MaximumSlotDuration)
        {
            ModelState.AddModelError(nameof(SlotCreateModel.EndTime), "A slot can last at most 4 hours.");
        }

        if (ModelState.IsValid)
        {
            var overlapsExistingSlot = await databaseContext.AvailabilitySlots.AnyAsync(slot =>
                slot.ConsultantProfileId == profile.Id
                && slot.StartTime < input.EndTime
                && slot.EndTime > input.StartTime);

            if (overlapsExistingSlot)
            {
                ModelState.AddModelError(string.Empty, "This slot overlaps with one of your existing slots.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        databaseContext.AvailabilitySlots.Add(new AvailabilitySlot
        {
            ConsultantProfileId = profile.Id,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            IsBooked = false
        });

        await databaseContext.SaveChangesAsync();
        logger.LogInformation("Consultant profile {ConsultantProfileId} added a slot starting at {StartTime}", profile.Id, input.StartTime);

        TempData["SuccessMessage"] = "The slot has been added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var profile = await FindCurrentProfileAsync();
        if (profile == null)
        {
            return NotFound();
        }

        var slot = await databaseContext.AvailabilitySlots.FirstOrDefaultAsync(existingSlot => existingSlot.Id == id);
        if (slot == null)
        {
            return NotFound();
        }

        if (slot.ConsultantProfileId != profile.Id)
        {
            return Forbid();
        }

        var hasBookingHistory = await databaseContext.Bookings.AnyAsync(booking => booking.AvailabilitySlotId == id);
        if (slot.IsBooked || hasBookingHistory)
        {
            TempData["ErrorMessage"] = "Slots that are booked or have booking history cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        databaseContext.AvailabilitySlots.Remove(slot);
        await databaseContext.SaveChangesAsync();
        logger.LogInformation("Consultant profile {ConsultantProfileId} deleted slot {AvailabilitySlotId}", profile.Id, id);

        TempData["SuccessMessage"] = "The slot has been deleted.";
        return RedirectToAction(nameof(Index));
    }

    private Task<ConsultantProfile?> FindCurrentProfileAsync()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return databaseContext.ConsultantProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.UserId == currentUserId);
    }
}
