using Microsoft.AspNetCore.Mvc;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.Models.DTOs.Dashboard;

namespace NovaStaff.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("kpi-summary")]
    [ProducesResponseType(typeof(KpiSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKpiSummary(CancellationToken ct)
    {
        var result = await _dashboardService.GetKpiSummaryAsync(ct);
        return Ok(result);
    }

    
    [HttpGet("employee-trends")]
    [ProducesResponseType(typeof(List<EmployeeTrendDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeTrends(
        [FromQuery] int limit = 6,
        CancellationToken ct = default)
    {
        if (limit is < 1 or > 24)
            return BadRequest("limit phải từ 1 đến 24");

        var result = await _dashboardService.GetEmployeeTrendsAsync(limit, ct);
        return Ok(result);
    }
}