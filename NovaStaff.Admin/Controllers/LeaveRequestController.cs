// Controllers/LeaveRequestController.cs
using Microsoft.AspNetCore.Mvc;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.Models.DTOs.LeaveRequest;

namespace NovaStaff.Controllers;

[ApiController]
[Route("api/leave-requests")]
public class LeaveRequestController : ControllerBase
{
    private readonly ILeaveRequestService _service;

    public LeaveRequestController(ILeaveRequestService service)
    {
        _service = service;
    }

    // =========================================================
    // READ
    // =========================================================

    /// <summary>
    /// Lấy danh sách đơn nghỉ của bản thân (current user).
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyRequests(CancellationToken ct)
    {
        var result = await _service.GetMyRequestsAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách đơn nghỉ theo employeeId.
    /// </summary>
    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployee(
        int employeeId,
        CancellationToken ct)
    {
        var result = await _service.GetByEmployeeAsync(employeeId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách đơn đang Pending. HR/Manager dùng.
    /// departmentId optional — nếu không truyền thì lấy tất cả.
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(
        [FromQuery] int? departmentId,
        CancellationToken ct)
    {
        var result = await _service.GetPendingAsync(departmentId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Tổng số ngày nghỉ đã được duyệt của nhân viên trong năm.
    /// </summary>
    [HttpGet("employee/{employeeId:int}/approved-days")]
    public async Task<IActionResult> GetApprovedDays(
        int employeeId,
        [FromQuery] int year,
        CancellationToken ct)
    {
        var result = await _service.GetApprovedDaysAsync(employeeId, year, ct);
        return Ok(new { employeeId, year, approvedDays = result });
    }

    // =========================================================
    // CREATE
    // =========================================================

    /// <summary>
    /// Tạo đơn xin nghỉ mới.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateLeaveRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(
            nameof(GetByEmployee),
            new { employeeId = result.EmployeeId },
            result);
    }

    // =========================================================
    // APPROVAL FLOW
    // =========================================================

    /// <summary>
    /// Duyệt đơn nghỉ.
    /// </summary>
    [HttpPost("{requestId:int}/approve")]
    public async Task<IActionResult> Approve(
        int requestId,
        CancellationToken ct)
    {
        await _service.ApproveAsync(requestId, ct);
        return NoContent();
    }

    /// <summary>
    /// Từ chối đơn nghỉ.
    /// </summary>
    [HttpPost("{requestId:int}/reject")]
    public async Task<IActionResult> Reject(
        int requestId,
        [FromBody] RejectLeaveRequest? request,
        CancellationToken ct)
    {
        await _service.RejectAsync(requestId, request?.Reason, ct);
        return NoContent();
    }
}