using System.Security.Claims;
using FrameRent.Application.DTOs.Rentals;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Constants;
using FrameRent.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Web.Controllers;

[Authorize(Roles = Roles.Customer)]
public class RentalsController : Controller
{
    private readonly IRentalService _rentalService;

    public RentalsController(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    [HttpGet]
    public async Task<IActionResult> MyRentals()
    {
        var userId = GetCurrentUserId();
        var rentals = await _rentalService.GetMyRentalsAsync(userId);
        return View(rentals);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid equipmentId, int quantity, DateOnly startDate, DateOnly endDate)
    {
        var userId = GetCurrentUserId();

        try
        {
            var created = await _rentalService.CreateRentalAsync(
                userId,
                new CreateRentalRequestDto(equipmentId, quantity, startDate, endDate));

            TempData["SuccessMessage"] = $"Rental request submitted! Total cost: ${created.TotalCost:F2} ({created.TotalDays} days). Status: {created.Status}.";
            return RedirectToAction(nameof(MyRentals));
        }
        catch (Exception ex) when (ex is DomainValidationException or NotFoundException or ConflictException)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Catalog", new { id = equipmentId, from = startDate, to = endDate });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetCurrentUserId();

        try
        {
            await _rentalService.CancelRentalAsync(id, userId);
            TempData["SuccessMessage"] = "Rental has been cancelled successfully.";
        }
        catch (Exception ex) when (ex is DomainValidationException or NotFoundException or ForbiddenAccessException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(MyRentals));
    }

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
