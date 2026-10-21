using FrameRent.Application.Interfaces;
using FrameRent.Domain.Exceptions;
using FrameRent.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Web.Controllers;

public class CatalogController : Controller
{
    private readonly IEquipmentService _equipmentService;

    public CatalogController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? category = null)
    {
        var items = await _equipmentService.GetActiveCatalogAsync();

        ViewBag.Categories = items.Select(e => e.Category).Distinct().OrderBy(c => c).ToList();
        ViewBag.SelectedCategory = category;

        if (!string.IsNullOrWhiteSpace(category))
        {
            items = items.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, DateOnly? from = null, DateOnly? to = null)
    {
        try
        {
            var equipment = await _equipmentService.GetByIdAsync(id);
            var checkFrom = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
            var checkTo = to ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4));

            var viewModel = new EquipmentDetailViewModel
            {
                Equipment = equipment,
                CheckFrom = checkFrom,
                CheckTo = checkTo
            };

            if (checkTo > checkFrom)
            {
                viewModel.Availability = await _equipmentService.GetAvailabilityAsync(id, checkFrom, checkTo);
            }
            else
            {
                TempData["ErrorMessage"] = "End date must be strictly after start date to check availability.";
            }

            return View(viewModel);
        }
        catch (NotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
