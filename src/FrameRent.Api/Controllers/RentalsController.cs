using System.Security.Claims;
using FrameRent.Application.DTOs.Rentals;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Api.Controllers;

[ApiController]
[Route("api/v1/rentals")]
[Authorize]
public class RentalsController : ControllerBase
{
    private readonly IRentalService _rentalService;

    public RentalsController(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<RentalResponseDto>> Create([FromBody] CreateRentalRequestDto request)
    {
        var userId = GetCurrentUserId();
        var rental = await _rentalService.CreateRentalAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = rental.Id }, rental);
    }

    [HttpGet("mine")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<IEnumerable<RentalResponseDto>>> GetMine()
    {
        var userId = GetCurrentUserId();
        var rentals = await _rentalService.GetMyRentalsAsync(userId);
        return Ok(rentals);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RentalResponseDto>> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var isStaff = User.IsInRole(Roles.Staff);
        var rental = await _rentalService.GetRentalByIdAsync(id, userId, isStaff);
        return Ok(rental);
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<RentalResponseDto>> Cancel(Guid id)
    {
        var userId = GetCurrentUserId();
        var rental = await _rentalService.CancelRentalAsync(id, userId);
        return Ok(rental);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<IEnumerable<RentalResponseDto>>> GetAll([FromQuery] string? status = null)
    {
        var rentals = await _rentalService.GetAllRentalsAsync(status);
        return Ok(rentals);
    }

    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<RentalResponseDto>> Approve(Guid id)
    {
        var rental = await _rentalService.ApproveRentalAsync(id);
        return Ok(rental);
    }

    [HttpPatch("{id:guid}/reject")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<RentalResponseDto>> Reject(Guid id)
    {
        var rental = await _rentalService.RejectRentalAsync(id);
        return Ok(rental);
    }

    [HttpPatch("{id:guid}/deliver")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<RentalResponseDto>> Deliver(Guid id)
    {
        var rental = await _rentalService.DeliverRentalAsync(id);
        return Ok(rental);
    }

    [HttpPatch("{id:guid}/return")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<RentalResponseDto>> Return(Guid id)
    {
        var rental = await _rentalService.ReturnRentalAsync(id);
        return Ok(rental);
    }

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
