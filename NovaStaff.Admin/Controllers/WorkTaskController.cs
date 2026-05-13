using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.Models.DTOs.WorkTasks;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Filters;
using NovaStaff.Services.Interfaces;

namespace NovaStaff.API.Controllers;

[Route("api/work-tasks")]
[ApiController]
[Authorize] // Tất cả endpoint đều cần đăng nhập
public class WorkTaskController : ControllerBase
{
    private readonly IWorkTaskService _workTaskService;
    private readonly ICurrentUserService _currentUser;

    public WorkTaskController(
        IWorkTaskService workTaskService,
        ICurrentUserService currentUser) 
    {
        _workTaskService = workTaskService;
        _currentUser = currentUser;
    }

    // ==========================================
    // LẤY DỮ LIỆU (GET)
    // ==========================================

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _workTaskService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] WorkTaskFilter filter,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _workTaskService.GetPagedAsync(filter, pageIndex, pageSize, ct);
        return Ok(result);
    }
    [Authorize(Roles = "Admin")]
    [HttpGet("assignee/{employeeId}")]
    public async Task<IActionResult> GetByAssignee(
        int employeeId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _workTaskService.GetByAssigneeAsync(employeeId, pageIndex, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("my-tasks")]
    [Authorize] // Tất cả role đều vào được
    public async Task<IActionResult> GetMyTasks(
    [FromQuery] int pageIndex = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken ct = default)
    {
        // Lấy employeeId từ JWT claim, không cho phép client truyền vào
        var employeeId = _currentUser.GetUserId()
            ?? throw new UnauthorizedAccessException("Unauthenticated");

        var result = await _workTaskService.GetByAssigneeAsync(employeeId, pageIndex, pageSize, ct);
        return Ok(result);
    }


    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueTasks(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _workTaskService.GetOverdueTasksAsync(pageIndex, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("manager/{managerId}")]
    [Authorize(Roles = "Admin,Manager")] // Chỉ Admin và Manager xem được tasks của team
    public async Task<IActionResult> GetByManager(
        int managerId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _workTaskService.GetByManagerAsync(managerId, pageIndex, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Manager")] // Thống kê chỉ dành cho Admin và Manager
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken ct = default)
    {
        var result = await _workTaskService.GetStatusStatisticsAsync(startDate, endDate, ct);
        return Ok(result);
    }

    // ==========================================
    // THÊM / SỬA / XÓA (POST, PUT, DELETE)
    // ==========================================

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")] // Chỉ Admin và Manager tạo task
    public async Task<IActionResult> Create([FromBody] CreateWorkTaskRequest request, CancellationToken ct)
    {
        var result = await _workTaskService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")] // Chỉ Admin và Manager sửa task
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkTaskRequest request, CancellationToken ct)
    {
        var result = await _workTaskService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")] // Chỉ Admin và Manager xóa task
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _workTaskService.DeleteAsync(id, ct);
        return NoContent();
    }

    // ==========================================
    // DOMAIN ACTION (PATCH)
    // ==========================================

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Manager")] // Chỉ Admin và Manager đổi trạng thái
    public async Task<IActionResult> ChangeStatus(
        int id,
        [FromBody] ChangeTaskStatusRequest request,
        CancellationToken ct)
    {
        await _workTaskService.ChangeStatusAsync(id, request.Status, ct);
        return NoContent();
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> CompleteTask(int id, CancellationToken ct)
    {
        // Tất cả role đều có thể chốt ngày hoàn thành (Staff tự chốt task của mình)
        await _workTaskService.CompleteTaskAsync(id, ct);
        return NoContent();
    }
}

public class ChangeTaskStatusRequest
{
    public WorkTaskStatus Status { get; set; }
}