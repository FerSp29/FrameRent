using FrameRent.Application.DTOs.Equipment;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize(Roles = Roles.Staff)]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("topequipment")]
    [HttpGet("top-equipment")]
    public async Task<ActionResult<IEnumerable<TopEquipmentReportDto>>> GetTopEquipment()
    {
        var report = await _reportService.GetTopEquipmentAsync();
        return Ok(report);
    }
}
