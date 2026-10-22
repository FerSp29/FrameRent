using FrameRent.Application.DTOs.Equipment;
using FrameRent.Application.DTOs.Rentals;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Constants;
using FrameRent.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Web.Controllers;

[Authorize(Roles = Roles.Staff)]
public class StaffController : Controller
{
    private readonly IRentalService _rentalService;
    private readonly IEquipmentService _equipmentService;

    public StaffController(IRentalService rentalService, IEquipmentService equipmentService)
    {
        _rentalService = rentalService;
        _equipmentService = equipmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? status = null)
    {
        var rentals = await _rentalService.GetAllRentalsAsync(status);
        var catalog = await _equipmentService.GetActiveCatalogAsync();

        ViewBag.StatusFilter = status;
        ViewBag.CatalogStock = catalog;

        return View(rentals);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            await _rentalService.ApproveRentalAsync(id);
            TempData["SuccessMessage"] = "Rental request has been approved successfully.";
        }
        catch (Exception ex) when (ex is DomainValidationException or ConflictException or NotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id)
    {
        try
        {
            await _rentalService.RejectRentalAsync(id);
            TempData["SuccessMessage"] = "Rental request has been rejected (due to insufficient stock or policy).";
        }
        catch (Exception ex) when (ex is DomainValidationException or NotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deliver(Guid id)
    {
        try
        {
            await _rentalService.DeliverRentalAsync(id);
            TempData["SuccessMessage"] = "Equipment delivered to customer successfully.";
        }
        catch (Exception ex) when (ex is DomainValidationException or NotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(Guid id)
    {
        try
        {
            var result = await _rentalService.ReturnRentalAsync(id);
            var lateMsg = result.LateFee > 0 ? $" (Late fee applied: ${result.LateFee:F2})" : " (Returned on time, no late fee).";
            TempData["SuccessMessage"] = $"Equipment return registered successfully!{lateMsg}";
        }
        catch (Exception ex) when (ex is DomainValidationException or NotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
