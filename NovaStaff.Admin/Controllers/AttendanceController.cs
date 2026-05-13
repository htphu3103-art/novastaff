// Controllers/AttendanceController.cs
using Microsoft.AspNetCore.Mvc;
using NovaStaff.Models.DTOs.Attendance;
using NovaStaff.Models.Filters;
using NovaStaff.Services.Interfaces;

namespace NovaStaff.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    // =========================================================
    // READ
    // =========================================================
    /// <summary>
    /// Lấy record chấm công hôm nay của nhân viên.
    /// Trả 204 nếu chưa check-in.
    /// </summary>
    [HttpGet("today/{employeeId:int}")]
    public async Task<IActionResult> GetToday(
        int employeeId,
        CancellationToken ct)
    {
        var result = await _attendanceService.GetTodayAsync(employeeId, ct);
        return result == null ? NoContent() : Ok(result);
    }

    /// <summary>
    /// Lấy bảng công theo tháng của nhân viên.
    /// </summary>
    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployeeAndMonth(
        int employeeId,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct)
    {
        var result = await _attendanceService.GetByEmployeeAndMonthAsync(employeeId, year, month, ct);
        return Ok(result);
    }

    /// <summary>
    /// Tổng giờ làm trong tháng của nhân viên.
    /// </summary>
    [HttpGet("employee/{employeeId:int}/total-hours")]
    public async Task<IActionResult> GetTotalHours(
        int employeeId,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct)
    {
        var result = await _attendanceService.GetTotalHoursAsync(employeeId, year, month, ct);
        return Ok(new { employeeId, year, month, totalHours = result });
    }
    [HttpGet("me/total-hours")]
    public async Task<IActionResult> GetMyTotalHours(
    [FromQuery] int year,
    [FromQuery] int month,
    CancellationToken ct)
    {
        var result = await _attendanceService.GetMyTotalHoursAsync(year, month, ct);

        return Ok(new
        {
            year,
            month,
            totalHours = result
        });
    }
    /// <summary>
    /// HR: Danh sách chấm công có phân trang + filter.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] AttendanceFilter filter,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _attendanceService.GetPagedAsync(filter, pageIndex, pageSize, ct);
        return Ok(result);
    }

    // =========================================================
    // ACTIONS — Check-in / Check-out
    // =========================================================

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn(CancellationToken ct = default)
    {
        var result = await _attendanceService.CheckInAsync(ct);
        return Ok(result);
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut(CancellationToken ct)
    {
        var result = await _attendanceService.CheckOutAsync(ct);
        return Ok(result);
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday(CancellationToken ct)
    {
        var result = await _attendanceService.GetTodayForCurrentUserAsync(ct);
        return result == null ? NoContent() : Ok(result);
    }

    /// <summary>
    /// Nhân viên check-in.
    /// </summary>
    [HttpPost("check-in/{employeeId:int}")]
    public async Task<IActionResult> CheckIn(
        int employeeId,
        CancellationToken ct)
    {
        var result = await _attendanceService.CheckInAsync(employeeId, ct);
        return CreatedAtAction(nameof(GetToday), new { employeeId }, result);
    }

    /// <summary>
    /// Nhân viên check-out.
    /// </summary>
    [HttpPost("check-out/{employeeId:int}")]
    public async Task<IActionResult> CheckOut(
        int employeeId,
        CancellationToken ct)
    {
        var result = await _attendanceService.CheckOutAsync(employeeId, ct);
        return Ok(result);
    }

    // =========================================================
    // HR MANAGEMENT
    // =========================================================

    /// <summary>
    /// HR tạo record thủ công.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateManual(
        [FromBody] CreateAttendanceRequest request,
        CancellationToken ct)
    {
        var result = await _attendanceService.CreateManualAsync(request, ct);
        return CreatedAtAction(nameof(GetToday), new { employeeId = result.EmployeeId }, result);
    }

    /// <summary>
    /// HR chỉnh sửa record.
    /// </summary>
    [HttpPut("{recordId:long}")]
    public async Task<IActionResult> Update(
        long recordId,
        [FromBody] UpdateAttendanceRequest request,
        CancellationToken ct)
    {
        var result = await _attendanceService.UpdateAsync(recordId, request, ct);
        return Ok(result);
    }

    /// <summary>
    /// HR xóa record.
    /// </summary>
    [HttpDelete("{recordId:long}")]
    public async Task<IActionResult> Delete(
        long recordId,
        CancellationToken ct)
    {
        await _attendanceService.DeleteAsync(recordId, ct);
        return NoContent();
    }
}