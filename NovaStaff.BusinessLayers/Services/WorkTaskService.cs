using Microsoft.EntityFrameworkCore;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.WorkTasks;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Filters;
using NovaStaff.Services.Interfaces;
using System.Linq.Expressions;

namespace NovaStaff.BusinessLayers.Services;

public class WorkTaskService : IWorkTaskService
{
    private readonly IWorkTaskRepository _workTaskRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUser;

    public WorkTaskService(
        IWorkTaskRepository workTaskRepo,
        IEmployeeRepository employeeRepo,
        IUnitOfWork uow,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUser)
    {
        _workTaskRepo = workTaskRepo;
        _employeeRepo = employeeRepo;
        _uow = uow;
        _dateTimeService = dateTimeService;
        _currentUser = currentUser;
    }

    private static WorkTaskDto MapToDto(WorkTask t) => new()
    {
        Id = t.TaskID, // Hoặc t.WorkTaskID tùy cách bạn đặt tên
        Title = t.Title,
        Description = t.Description,
        Status = t.Status.ToString(),
        Priority = t.Priority.ToString(),
        DueDate = t.DueDate,
        CreatedDate = t.CreatedDate,
        EmployeeId = t.EmployeeID,
        CompletedDate = t.CompletedDate,
        AssigneeName = t.Employee?.FullName
    };

    // ========================= READ =========================

    public async Task<WorkTaskDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var task = await _workTaskRepo.GetByIdAsync(
            id,
            trackChanges: false,
            include: q => q.Include(x => x.Employee),
            ct: ct);

        if (task == null)
            throw new KeyNotFoundException($"Không tìm thấy công việc ID {id}");

        return MapToDto(task);
    }

    public async Task<PagedResult<WorkTaskDto>> GetPagedAsync(
        WorkTaskFilter filter, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        filter ??= new WorkTaskFilter();

        if (!_currentUser.IsAuthenticated())
            throw new UnauthorizedAccessException("Unauthenticated");

        var role = _currentUser.GetRole();
        var currentEmployeeId = _currentUser.GetEmployeeId();
        var isAdmin = string.Equals(role, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase);
        var isManager = string.Equals(role, UserRole.Manager.ToString(), StringComparison.OrdinalIgnoreCase);
        var isStaff = string.Equals(role, UserRole.Staff.ToString(), StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && !isManager && !isStaff)
            throw new UnauthorizedAccessException("Role is not allowed to access work tasks.");

        if ((isManager || isStaff) && !currentEmployeeId.HasValue)
            throw new UnauthorizedAccessException("Current account is missing EmployeeId.");

        Expression<Func<WorkTask, bool>> predicate = x =>
            (string.IsNullOrEmpty(filter.TitleContains) || x.Title.Contains(filter.TitleContains)) &&
            (!filter.Status.HasValue || x.Status == filter.Status) &&
            (!filter.Priority.HasValue || x.Priority == filter.Priority) &&
            (!filter.EmployeeId.HasValue || x.EmployeeID == filter.EmployeeId) &&
            (isAdmin ||
             (isManager && x.Employee != null && x.Employee.SupervisorID == currentEmployeeId!.Value) ||
             (isStaff && x.EmployeeID == currentEmployeeId!.Value));

        var result = await _workTaskRepo.GetPagedAsync(
            pageIndex, pageSize,
            filter: predicate,
            orderBy: q => q.OrderByDescending(x => x.CreatedDate),
            include: q => q.Include(x => x.Employee),
            trackChanges: false, ct: ct);

        return new PagedResult<WorkTaskDto>(
            result.Items.Select(MapToDto).ToList(),
            result.TotalCount, result.PageIndex, result.PageSize);
    }

    public async Task<PagedResult<WorkTaskDto>> GetByAssigneeAsync(
        int employeeId, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        var result = await _workTaskRepo.GetByAssigneePagedAsync(
            employeeId, pageIndex, pageSize,
            include: q => q.Include(x => x.Employee),
            trackChanges: false, ct: ct);

        return new PagedResult<WorkTaskDto>(
            result.Items.Select(MapToDto).ToList(), result.TotalCount, pageIndex, pageSize);
    }

    public async Task<PagedResult<WorkTaskDto>> GetOverdueTasksAsync(
        int pageIndex, int pageSize, CancellationToken ct = default)
    {
        var now = _dateTimeService.LocalNow;
        var result = await _workTaskRepo.GetOverduePagedAsync(
            now, pageIndex, pageSize,
            include: q => q.Include(x => x.Employee),
            trackChanges: false, ct: ct);

        return new PagedResult<WorkTaskDto>(
            result.Items.Select(MapToDto).ToList(), result.TotalCount, pageIndex, pageSize);
    }

    public async Task<PagedResult<WorkTaskDto>> GetByManagerAsync(
        int managerId, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        var result = await _workTaskRepo.GetByManagerPagedAsync(
            managerId, pageIndex, pageSize,
            include: q => q.Include(x => x.Employee).ThenInclude(e => e!.Department),
            trackChanges: false, ct: ct);

        return new PagedResult<WorkTaskDto>(
            result.Items.Select(MapToDto).ToList(), result.TotalCount, pageIndex, pageSize);
    }

    public async Task<Dictionary<string, int>> GetStatusStatisticsAsync(
        DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var stats = await _workTaskRepo.CountByStatusAsync(startDate, endDate, ct);
        return stats.ToDictionary(k => k.Key.ToString(), v => v.Value);
    }

    // ========================= WRITE =========================

    public async Task<WorkTaskDto> CreateAsync(CreateWorkTaskRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Tiêu đề công việc không được để trống.");

        if (request.EmployeeId.HasValue && !await _employeeRepo.ExistsAsync(request.EmployeeId.Value, ct))
            throw new KeyNotFoundException("Nhân viên được giao việc không tồn tại.");

        var task = new WorkTask
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = WorkTaskStatus.Todo,
            Priority = request.Priority,
            DueDate = request.DueDate,
            EmployeeID = request.EmployeeId,
            CreatedDate = _dateTimeService.LocalNow
        };

        await _workTaskRepo.AddAsync(task, ct);
        await _uow.SaveChangesAsync(ct); // ← Interceptor tự ghi
        return MapToDto(task);
    }

    public async Task<WorkTaskDto> UpdateAsync(int id, UpdateWorkTaskRequest request, CancellationToken ct = default)
    {
        var task = await _workTaskRepo.GetByIdAsync(id, trackChanges: true, ct: ct);
        if (task == null)
            throw new KeyNotFoundException("Công việc không tồn tại.");

        if (request.EmployeeId.HasValue && request.EmployeeId != task.EmployeeID)
        {
            if (!await _employeeRepo.ExistsAsync(request.EmployeeId.Value, ct))
                throw new KeyNotFoundException("Nhân viên được giao việc không tồn tại.");
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.EmployeeID = request.EmployeeId;
        if (request.Status.HasValue) task.Status = request.Status.Value;

        _workTaskRepo.Update(task);
        await _uow.SaveChangesAsync(ct); // ← Interceptor tự ghi
        return MapToDto(task);
    }
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var task = await _workTaskRepo.GetByIdAsync(id, trackChanges: true, ct: ct);
        if (task == null)
            throw new KeyNotFoundException("Công việc không tồn tại.");

        _workTaskRepo.Delete(task);
        await _uow.SaveChangesAsync(ct); // ← Interceptor tự ghi
    }
    // ========================= DOMAIN LOGIC =========================

    public async Task ChangeStatusAsync(int id, WorkTaskStatus newStatus, CancellationToken ct = default)
    {
        var task = await _workTaskRepo.GetByIdAsync(id, trackChanges: true, ct: ct);
        if (task == null)
            throw new KeyNotFoundException("Công việc không tồn tại.");

        if (task.Status == newStatus)
            return;
        task.Status = newStatus;

        _workTaskRepo.Update(task);

        await _uow.SaveChangesAsync(ct);
    }

    public async Task CompleteTaskAsync(int id, CancellationToken ct = default)
    {
        var task = await _workTaskRepo.GetByIdAsync(id, trackChanges: true, ct: ct);
        if (task == null)
            throw new KeyNotFoundException("Công việc không tồn tại.");

        if (task.Status != WorkTaskStatus.Done)
            throw new InvalidOperationException("Chỉ được chốt ngày hoàn thành khi công việc ở trạng thái Done.");

        if (task.CompletedDate.HasValue)
            return;

        var completedDate = _dateTimeService.LocalNow;
        task.CompletedDate = completedDate;

        _workTaskRepo.Update(task);

        await _uow.SaveChangesAsync(ct);
    }
}

