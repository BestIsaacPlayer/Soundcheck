using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;
using Soundcheck.Web.Services;

namespace Soundcheck.Web.Controllers;

[Authorize(Roles = "Consultant")]
public class ConsultantBookingsController(ApplicationDbContext databaseContext, IBookingService bookingService)
    : Controller
{
    public async Task<IActionResult> Index()
    {
        var currentUserId = GetCurrentUserId();

        var bookings = await databaseContext.Bookings
            .AsNoTracking()
            .Include(booking => booking.Client)
            .Include(booking => booking.EquipmentCategory)
            .Include(booking => booking.AvailabilitySlot)
            .Where(booking => booking.AvailabilitySlot.ConsultantProfile.UserId == currentUserId)
            .OrderByDescending(booking => booking.AvailabilitySlot.StartTime)
            .ToListAsync();

        return View(bookings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        var result = await bookingService.ConfirmBookingAsync(id, GetCurrentUserId());
        SetResultMessage(result, "The booking has been confirmed.");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var result = await bookingService.CompleteBookingAsync(id, GetCurrentUserId());
        SetResultMessage(result, "The consultation has been marked as completed.");

        return RedirectToAction(nameof(Index));
    }

    private void SetResultMessage(BookingResult result, string successMessage)
    {
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = successMessage;
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("The current user has no identifier.");
    }
}