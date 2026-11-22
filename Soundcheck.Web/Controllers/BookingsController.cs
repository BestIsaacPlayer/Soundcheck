using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;
using Soundcheck.Web.Models;
using Soundcheck.Web.Services;
using Soundcheck.Web.ViewModels;

namespace Soundcheck.Web.Controllers;

[Authorize]
public class BookingsController(ApplicationDbContext databaseContext, IBookingService bookingService)
    : Controller
{
    public async Task<IActionResult> Index()
    {
        var currentUserId = GetCurrentUserId();

        var bookings = await databaseContext.Bookings
            .AsNoTracking()
            .Include(booking => booking.EquipmentCategory)
            .Include(booking => booking.Review)
            .Include(booking => booking.AvailabilitySlot)
                .ThenInclude(slot => slot.ConsultantProfile)
                    .ThenInclude(profile => profile.User)
            .Where(booking => booking.ClientId == currentUserId)
            .OrderByDescending(booking => booking.AvailabilitySlot.StartTime)
            .ToListAsync();

        return View(bookings);
    }

    public async Task<IActionResult> Create(int slotId)
    {
        var viewModel = await BuildCreateViewModelAsync(slotId, null);
        if (viewModel == null)
        {
            return NotFound();
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnCreateViewAsync(input);
        }

        var result = await bookingService.BookConsultationAsync(
            GetCurrentUserId(),
            input.AvailabilitySlotId,
            input.EquipmentCategoryId,
            input.Topic.Trim());

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The consultation could not be booked.");
            return await ReturnCreateViewAsync(input);
        }

        TempData["SuccessMessage"] = "Your consultation has been booked.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var booking = await databaseContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(existingBooking => existingBooking.Id == id);

        if (booking == null)
        {
            return NotFound();
        }

        if (booking.ClientId != GetCurrentUserId())
        {
            return Forbid();
        }

        var result = await bookingService.CancelBookingAsync(id);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "The booking has been cancelled.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Review(int id)
    {
        var viewModel = await BuildReviewViewModelAsync(id, null);
        if (viewModel == null)
        {
            return NotFound();
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(BookingReviewInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnReviewViewAsync(input);
        }

        var result = await bookingService.AddReviewAsync(
            input.BookingId,
            GetCurrentUserId(),
            input.Rating,
            input.Comment);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The review could not be saved.");
            return await ReturnReviewViewAsync(input);
        }

        TempData["SuccessMessage"] = "Thank you for your review.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReturnCreateViewAsync(BookingCreateInputModel input)
    {
        var viewModel = await BuildCreateViewModelAsync(input.AvailabilitySlotId, input);
        if (viewModel == null)
        {
            return NotFound();
        }

        return View(viewModel);
    }

    private async Task<IActionResult> ReturnReviewViewAsync(BookingReviewInputModel input)
    {
        var viewModel = await BuildReviewViewModelAsync(input.BookingId, input);
        if (viewModel == null)
        {
            return NotFound();
        }

        return View(viewModel);
    }

    private async Task<BookingCreateViewModel?> BuildCreateViewModelAsync(int slotId, BookingCreateInputModel? input)
    {
        var availabilitySlot = await databaseContext.AvailabilitySlots
            .AsNoTracking()
            .Include(slot => slot.ConsultantProfile)
                .ThenInclude(profile => profile.User)
            .Include(slot => slot.ConsultantProfile)
                .ThenInclude(profile => profile.ConsultantCategories)
                    .ThenInclude(consultantCategory => consultantCategory.EquipmentCategory)
            .FirstOrDefaultAsync(slot => slot.Id == slotId);

        if (availabilitySlot == null)
        {
            return null;
        }

        return new BookingCreateViewModel
        {
            Input = input ?? new BookingCreateInputModel { AvailabilitySlotId = slotId },
            ConsultantName = availabilitySlot.ConsultantProfile.User.FullName,
            StartTime = availabilitySlot.StartTime,
            EndTime = availabilitySlot.EndTime,
            Categories = availabilitySlot.ConsultantProfile.ConsultantCategories
                .Select(consultantCategory => consultantCategory.EquipmentCategory)
                .OrderBy(category => category.Name)
                .ToList()
        };
    }

    private async Task<BookingReviewViewModel?> BuildReviewViewModelAsync(int bookingId, BookingReviewInputModel? input)
    {
        var currentUserId = GetCurrentUserId();

        var booking = await databaseContext.Bookings
            .AsNoTracking()
            .Include(bookingWithSlot => bookingWithSlot.AvailabilitySlot)
                .ThenInclude(slot => slot.ConsultantProfile)
                    .ThenInclude(profile => profile.User)
            .FirstOrDefaultAsync(candidate =>
                candidate.Id == bookingId
                && candidate.ClientId == currentUserId
                && candidate.Status == BookingStatus.Completed);

        if (booking == null)
        {
            return null;
        }

        return new BookingReviewViewModel
        {
            Input = input ?? new BookingReviewInputModel { BookingId = bookingId },
            ConsultantName = booking.AvailabilitySlot.ConsultantProfile.User.FullName,
            Topic = booking.Topic
        };
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("The current user has no identifier.");
    }
}
