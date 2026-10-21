using FrameRent.Application.DTOs.Equipment;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Api.Controllers;

[ApiController]
[Route("api/v1/equipment")]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;

    public EquipmentController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EquipmentResponseDto>>> GetAllActive()
    {
        var catalog = await _equipmentService.GetActiveCatalogAsync();
        return Ok(catalog);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<EquipmentResponseDto>> GetById(Guid id)
    {
        var equipment = await _equipmentService.GetByIdAsync(id);
        return Ok(equipment);
    }

    [HttpGet("{id:guid}/availability")]
    [AllowAnonymous]
    public async Task<ActionResult<EquipmentAvailabilityDto>> GetAvailability(
        Guid id,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to)
    {
        var availability = await _equipmentService.GetAvailabilityAsync(id, from, to);
        return Ok(availability);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<EquipmentResponseDto>> Create([FromBody] CreateEquipmentRequestDto request)
    {
        var created = await _equipmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<EquipmentResponseDto>> Update(Guid id, [FromBody] UpdateEquipmentRequestDto request)
    {
        var updated = await _equipmentService.UpdateAsync(id, request);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<EquipmentResponseDto>> Deactivate(Guid id)
    {
        var deactivated = await _equipmentService.DeactivateAsync(id);
        return Ok(deactivated);
    }
}
