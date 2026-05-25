using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.WorkTasks;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Filters;

namespace NovaStaff.Services.Interfaces;

public interface IWorkTaskService
{
    // READ
    Task<WorkTaskDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<WorkTaskDto>> GetPagedAsync(WorkTaskFilter filter, int pageIndex, int pageSize, CancellationToken ct = default);
    Task<PagedResult<WorkTaskDto>> GetByAssigneeAsync(int employeeId, int pageIndex, int pageSize, CancellationToken ct = default);
    Task<PagedResult<WorkTaskDto>> GetOverdueTasksAsync(int pageIndex, int pageSize, CancellationToken ct = default);
    Task<PagedResult<WorkTaskDto>> GetByManagerAsync(int managerId, int pageIndex, int pageSize, CancellationToken ct = default);
    Task<Dictionary<string, int>> GetStatusStatisticsAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    // WRITE
    Task<WorkTaskDto> CreateAsync(CreateWorkTaskRequest request, CancellationToken ct = default);
    Task<WorkTaskDto> UpdateAsync(int id, UpdateWorkTaskRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    // DOMAIN ACTION
    Task ChangeStatusAsync(int id, WorkTaskStatus newStatus, CancellationToken ct = default);
    Task CompleteTaskAsync(int id, CancellationToken ct = default);
}