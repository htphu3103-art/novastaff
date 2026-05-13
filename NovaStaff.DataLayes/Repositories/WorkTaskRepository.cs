using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Common;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;
using System.Linq.Expressions;

namespace NovaStaff.DataLayers.Repositories;

public class WorkTaskRepository : GenericRepository<WorkTask, int>, IWorkTaskRepository
{
    private readonly IDateTimeService _dateTimeService;
    public WorkTaskRepository(AppDbContext context, IDateTimeService dateTimeService)
        : base(context)
    {
        _dateTimeService = dateTimeService;
    }

    public async Task<PagedResult<WorkTask>> GetByAssigneePagedAsync(
        int employeeId,
        int pageIndex,
        int pageSize,
        Expression<Func<WorkTask, bool>>? predicate = null,
        Func<IQueryable<WorkTask>, IOrderedQueryable<WorkTask>>? orderBy = null,
        Func<IQueryable<WorkTask>, IQueryable<WorkTask>>? include = null,
        bool trackChanges = false,
        CancellationToken ct = default)
    {
        // 1. Khởi tạo query base và filter theo Assignee
        // Lưu ý: IsDeleted = false đã được xử lý ngầm nếu bạn dùng Global Query Filter trong DbContext
        var query = BuildQuery(trackChanges, include)
            .Where(x => x.EmployeeID == employeeId);

        // 2. Add thêm flexible predicate từ Service Layer (nếu có)
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        // 3. Đếm tổng số lượng (trước khi phân trang)
        var totalCount = await query.CountAsync(ct);

        // 4. Xử lý OrderBy (Dùng default OrderBy nếu Service không truyền vào)
        query = orderBy != null
            ? orderBy(query)
            : query.OrderByDescending(x => x.Priority).ThenBy(x => x.DueDate);

        // 5. Phân trang và lấy dữ liệu
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<WorkTask>(items, totalCount, pageIndex, pageSize);
    }

    public async Task<PagedResult<WorkTask>> GetOverduePagedAsync(
        DateTime now,
        int pageIndex,
        int pageSize,
        Expression<Func<WorkTask, bool>>? predicate = null,
        Func<IQueryable<WorkTask>, IOrderedQueryable<WorkTask>>? orderBy = null,
        Func<IQueryable<WorkTask>, IQueryable<WorkTask>>? include = null,
        bool trackChanges = false,
        CancellationToken ct = default)
    {
        var query = BuildQuery(trackChanges, include)
            .Where(x => x.DueDate.HasValue && x.DueDate < now);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync(ct);

        query = orderBy != null
            ? orderBy(query)
            : query.OrderByDescending(x => x.Priority).ThenBy(x => x.DueDate);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<WorkTask>(items, totalCount, pageIndex, pageSize);
    }

    public async Task<PagedResult<WorkTask>> GetByManagerPagedAsync(
        int managerEmployeeId,
        int pageIndex,
        int pageSize,
        Expression<Func<WorkTask, bool>>? predicate = null,
        Func<IQueryable<WorkTask>, IOrderedQueryable<WorkTask>>? orderBy = null,
        Func<IQueryable<WorkTask>, IQueryable<WorkTask>>? include = null,
        bool trackChanges = false,
        CancellationToken ct = default)
    {
        // Yêu cầu Entity WorkTask có Navigation Property là `Employee`
        // Yêu cầu Entity Employee có `SupervisorID` và Navigation Property `Department`
        var query = BuildQuery(trackChanges, include)
            .Where(x => x.EmployeeID != null &&
                        x.Employee != null &&
                        (x.Employee.SupervisorID == managerEmployeeId ||
                        (x.Employee.Department != null && x.Employee.Department.ManagerEmployeeID == managerEmployeeId)));

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync(ct);

        // Fallback default order để luôn có thứ tự nhất quán
        query = orderBy != null
            ? orderBy(query)
            : query.OrderByDescending(x => x.Priority).ThenBy(x => x.DueDate);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<WorkTask>(items, totalCount, pageIndex, pageSize);
    }

    public async Task<Dictionary<WorkTaskStatus, int>> CountByStatusAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
    {
        // Dùng AsNoTracking cho Query thống kê (chỉ đọc) để tối ưu memory
        // Yêu cầu có trường CreatedDate nằm trong BaseEntity
        var query = _dbSet.AsNoTracking()
            .Where(x => x.CreatedDate >= startDate && x.CreatedDate <= endDate)
            .GroupBy(x => x.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            });

        // Convert query kết quả sang Dictionary
        return await query.ToDictionaryAsync(x => x.Status, x => x.Count, ct);
    }

    public async Task<bool> UpdateCompletedDateAsync(
    int taskId,
    DateTime? completedDate,
    CancellationToken ct = default)
    {
        var affected = await _context.WorkTasks
            .Where(t => t.TaskID == taskId) // Global filter IsDeleted=false tự áp dụng
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(t => t.CompletedDate, completedDate)
                    .SetProperty(t => t.ModifiedDate, _dateTimeService.UtcNow),
                ct);

        return affected > 0;
    }
}