// Interfaces/Repositories/IWorkTaskRepository.cs
using NovaStaff.Models.Common;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;
using System.Linq.Expressions;

/*
?? METADATA - CRITICAL FOR AI IMPLEMENTATION:
TABLE: WorkTasks
PK: TaskID (int)
Key Fields: 
  - EmployeeID (int?) ? Assignee FK Employee.EmployeeID
  - DueDate (DateTime?) ? Overdue calculation  
  - Status (WorkTaskStatus) ? Todo/InProgress/Done
  - Priority (WorkTaskPriority) ? Low/Medium/High/Critical
  - Title (string, MaxLength 255, required)
GLOBAL FILTER: IsDeleted = false (BaseEntity)
*/

namespace NovaStaff.DataLayers.Interfaces.Repositories;

/// <summary>
/// Repository cho WorkTask - h? th?ng qu?n l? công vi?c/ticket.
///
/// WorkTask là core c?a productivity system.
/// Query volume cao ? ALWAYS pagination + index EmployeeID + DueDate.
///
/// Các field quan tr?ng:
///   TaskID      : int, khoá chính t? tăng
///   EmployeeID  : int?, FK ? Employee (assignee)
///   Title       : string required (MaxLength 255)
///   DueDate     : DateTime?, h?n hoàn thành
///   CompletedDate: DateTime?, th?i đi?m done
///   Status      : WorkTaskStatus enum (Todo/InProgress/Done/Cancelled)
///   Priority    : WorkTaskPriority enum (Low/Medium/High/Critical)
/// </summary>
public interface IWorkTaskRepository : IRepository<WorkTask, int>
{
    /// <summary>
    /// Query task theo ngư?i đư?c giao (assignee) — có phân trang.
    ///
    /// Index-optimized query:
    ///   WHERE EmployeeID = @employeeId AND IsDeleted = 0
    ///   ORDER BY [orderBy ho?c Priority DESC, DueDate ASC]
    ///
    /// Filter linh ho?t:
    ///   predicate: Status == Todo, Priority == High, DueDate range...
    ///
    /// Dùng khi:
    ///   - Kanban board cá nhân (My Tasks)
    ///   - Manager xem tasks c?a team member
    ///   - Dashboard: s? task pending theo employee
    ///
    /// Rule:
    ///   - Pagination b?t bu?c (high volume)
    ///   - Không filter business logic (Status "open") ? Service layer
    /// </summary>
    Task<PagedResult<WorkTask>> GetByAssigneePagedAsync(
        int employeeId,
        int pageIndex,
        int pageSize,
        Expression<Func<WorkTask, bool>>? predicate = null,
        Func<IQueryable<WorkTask>, IOrderedQueryable<WorkTask>>? orderBy = null,
        Func<IQueryable<WorkTask>, IQueryable<WorkTask>>? include = null,
        bool trackChanges = false,
        CancellationToken ct = default);

    /// <summary>
    /// Query task quá h?n d?a trên th?i gian hi?n t?i.
    ///
    /// Base query t?i ưu index DueDate:
    ///   WHERE DueDate < @now AND IsDeleted = 0
    ///
    /// @now t? IDateTimeService.UtcNow (testable, timezone safe).
    /// predicate b? sung: Status != Done, Priority = High, EmployeeID...
    ///
    /// Dùng khi:
    ///   - Dashboard "Overdue Tasks" cho manager
    ///   - Daily report: "X tasks missed deadline"
    ///   - Alert system: notify assignee
    ///
    /// Rule:
    ///   - KHÔNG filter "chưa hoàn thành" t?i repository
    ///   - Service layer thêm Status != Done/Cancelled
    ///   - Pagination b?t bu?c
    ///   - OrderBy default: Priority DESC, DueDate ASC
    /// </summary>
    Task<PagedResult<WorkTask>> GetOverduePagedAsync(
        DateTimeOffset now,
        int pageIndex,
        int pageSize,
        Expression<Func<WorkTask, bool>>? predicate = null,
        Func<IQueryable<WorkTask>, IOrderedQueryable<WorkTask>>? orderBy = null,
        Func<IQueryable<WorkTask>, IQueryable<WorkTask>>? include = null,
        bool trackChanges = false,
        CancellationToken ct = default);

    /// <summary>
    /// L?y task theo manager (bao g?m c? tasks c?a subordinates).
    ///
    /// Recursive query qua Employee.SupervisorID:
    ///   Tasks c?a Employee WHERE Employee.SupervisorID = @managerId
    ///   OR Employee.Department.ManagerEmployeeID = @managerId
    ///
    /// Dùng khi: Manager xem toàn b? work c?a team m?nh qu?n l?
    /// </summary>
    Task<PagedResult<WorkTask>> GetByManagerPagedAsync(
        int managerEmployeeId,
        int pageIndex,
        int pageSize,
        Expression<Func<WorkTask, bool>>? predicate = null,
        Func<IQueryable<WorkTask>, IOrderedQueryable<WorkTask>>? orderBy = null,
        Func<IQueryable<WorkTask>, IQueryable<WorkTask>>? include = null,
        bool trackChanges = false,
        CancellationToken ct = default);

    /// <summary>
    /// Đ?m s? task theo tr?ng thái trong kho?ng th?i gian.
    ///
    /// Aggregate query t?i ưu:
    ///   COUNT(*) GROUP BY Status 
    ///   WHERE CreatedDate BETWEEN @start AND @end
    ///
    /// Dùng khi: Dashboard KPI "Tasks completed this week"
    /// </summary>
    Task<Dictionary<WorkTaskStatus, int>> CountByStatusAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default);
    /// <summary>
    /// Cập nhật ngày hoàn thành của task.
    ///
    /// Chỉ update đúng field CompletedDate (và ModifiedDate qua BaseEntity).
    /// Không load toàn bộ entity để tránh unnecessary SELECT.
    ///
    /// Rules:
    ///   - completedDate = null → xóa ngày hoàn thành (reopen task)
    ///   - Không tự động đổi Status → Service layer chịu trách nhiệm
    ///   - Trả về false nếu TaskID không tồn tại
    /// </summary>
    Task<bool> UpdateCompletedDateAsync(
        int taskId,
        DateTime? completedDate,
        CancellationToken ct = default);
}



