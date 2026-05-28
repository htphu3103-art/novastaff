// API/Controllers/PayrollController.cs
using Microsoft.AspNetCore.Mvc;
using NovaStaff.Models.DTOs.Payroll;
using NovaStaff.Services.Interfaces;

namespace NovaStaff.API.Controllers;

[ApiController]
[Route("api/payroll")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    // ============================================================
    // PERIOD
    // ============================================================

    // GET api/payroll/periods?pageIndex=1&pageSize=12
    [HttpGet("periods")]
    public async Task<IActionResult> GetPeriods(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        var result = await _payrollService.GetPeriodsAsync(pageIndex, pageSize, ct);
        return Ok(result);
    }

    // GET api/payroll/periods/active
    [HttpGet("periods/active")]
    public async Task<IActionResult> GetActivePeriod(CancellationToken ct = default)
    {
        var result = await _payrollService.GetActivePeriodAsync(ct);
        if (result is null) return NoContent();
        return Ok(result);
    }

    // GET api/payroll/periods/5
    [HttpGet("periods/{periodId:int}")]
    public async Task<IActionResult> GetPeriodDetail(
        int periodId,
        [FromQuery] int? departmentId = null,
        CancellationToken ct = default)
    {
        var result = await _payrollService.GetPeriodDetailAsync(periodId, departmentId, ct);
        return Ok(result);
    }

    // POST api/payroll/periods
    [HttpPost("periods")]
    public async Task<IActionResult> CreatePeriod(
        [FromBody] CreatePayrollPeriodRequest request,
        CancellationToken ct = default)
    {
        var period = await _payrollService.CreatePeriodAsync(request, ct);
        return CreatedAtAction(nameof(GetPeriodDetail), new { periodId = period.PeriodID }, period);
    }

    // PUT api/payroll/periods/5/advance
    [HttpPut("periods/{periodId:int}/advance")]
    public async Task<IActionResult> AdvancePeriodStatus(
        int periodId,
        [FromBody] AdvancePeriodStatusRequest request,
        CancellationToken ct = default)
    {
        // Đảm bảo PeriodID từ route luôn thắng body
        request.PeriodID = periodId;

        var result = await _payrollService.AdvancePeriodStatusAsync(request, ct);
        return Ok(result);
    }

    // GET api/payroll/periods/5/summary
    [HttpGet("periods/{periodId:int}/summary")]
    public async Task<IActionResult> GetStatusSummary(int periodId, CancellationToken ct = default)
    {
        var result = await _payrollService.GetStatusSummaryAsync(periodId, ct);
        return Ok(result);
    }

    // GET api/payroll/periods/5/total
    [HttpGet("periods/{periodId:int}/total")]
    public async Task<IActionResult> GetTotalNetSalary(int periodId, CancellationToken ct = default)
    {
        var total = await _payrollService.GetTotalNetSalaryAsync(periodId, ct);
        return Ok(new { periodId, totalNetSalary = total });
    }

    // ============================================================
    // DETAIL (PAYSLIP)
    // ============================================================

    // GET api/payroll/periods/5/employees/10/payslip
    [HttpGet("periods/{periodId:int}/employees/{employeeId:int}/payslip")]
    public async Task<IActionResult> GetPayslip(
        int periodId,
        int employeeId,
        CancellationToken ct = default)
    {
        var result = await _payrollService.GetPayslipAsync(periodId, employeeId, ct);
        return Ok(result);
    }

    // POST api/payroll/periods/5/employees/10/calculate
    [HttpPost("periods/{periodId:int}/employees/{employeeId:int}/calculate")]
    public async Task<IActionResult> CalculatePayslip(
        int periodId,
        int employeeId,
        CancellationToken ct = default)
    {
        var request = new CalculatePayrollDetailRequest
        {
            PeriodID = periodId,
            EmployeeID = employeeId
        };

        var result = await _payrollService.CalculatePayslipAsync(request, ct);
        return Ok(result);
    }

    // POST api/payroll/periods/5/calculate-batch
    [HttpPost("periods/{periodId:int}/calculate-batch")]
    public async Task<IActionResult> BatchCalculate(
        int periodId,
        [FromBody] BatchCalculateRequest request,
        CancellationToken ct = default)
    {
        // PeriodID từ route luôn thắng body
        request.PeriodID = periodId;

        var result = await _payrollService.BatchCalculateAsync(request, ct);
        return Ok(result);
    }

    // PUT api/payroll/periods/5/employees/10/adjustments
    [HttpPut("periods/{periodId:int}/employees/{employeeId:int}/adjustments")]
    public async Task<IActionResult> UpdateAdjustments(
        int periodId,
        int employeeId,
        [FromBody] UpdatePayrollAdjustmentsRequest request,
        CancellationToken ct = default)
    {
        var result = await _payrollService.UpdateAdjustmentsAsync(
            periodId, employeeId, request, ct);
        return Ok(result);
    }
}