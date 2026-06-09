using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;

namespace NovaStaff.DataLayers.Repositories;

public class EmployeeRepository
    : GenericRepository<Employee, int>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context) { }

    public async Task<Employee?> GetByCodeAsync(
        string employeeCode,
        bool trackChanges = false,
        Func<IQueryable<Employee>, IQueryable<Employee>>? include = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
            return null;

        var query = trackChanges ? _dbSet : _dbSet.AsNoTracking();
        query = query.Where(e => e.EmployeeCode == employeeCode);

        if (include != null)
            query = include(query);

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsCodeUniqueAsync(
        string code,
        int? excludeId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return true;

        return !await _dbSet.AnyAsync(e =>
            e.EmployeeCode == code &&
            (!excludeId.HasValue || e.EmployeeID != excludeId.Value),
            ct);
    }

    public async Task<IEnumerable<Employee>> GetSubordinatesAsync(
        int managerId,
        bool trackChanges = false,
        Func<IQueryable<Employee>, IQueryable<Employee>>? include = null,
        CancellationToken ct = default)
    {
        var query = trackChanges ? _dbSet : _dbSet.AsNoTracking();
        query = query.Where(e => e.SupervisorID == managerId);

        if (include != null)
            query = include(query);

        return await query.OrderBy(e => e.FullName).ToListAsync(ct);
    }
    public async Task<Employee?> GetDetailByIdAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Supervisor)
            .FirstOrDefaultAsync(x => x.EmployeeID == id, ct);
    }
    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();

        return !await _dbSet.AnyAsync(e =>
            e.Email.ToLower() == normalized &&
            (!excludeId.HasValue || e.EmployeeID != excludeId.Value),
            ct);
    }

    public async Task<bool> IsPhoneUniqueAsync(string phone, int? excludeId, CancellationToken ct)
    {
        var normalized = phone.Trim();

        return !await _dbSet.AnyAsync(e =>
            e.Phone != null &&
            e.Phone == normalized &&
            (!excludeId.HasValue || e.EmployeeID != excludeId.Value),
            ct);
    }
    public async Task<IEnumerable<Employee>> GetManagersAsync(
    bool trackChanges = false,
    Func<IQueryable<Employee>, IQueryable<Employee>>? include = null,
    CancellationToken ct = default)
    {
        var query = trackChanges ? _dbSet : _dbSet.AsNoTracking();

        query = query.Where(e =>
            e.User != null && e.User.Role == UserRole.Manager);

        if (include != null)
            query = include(query);

        return await query
            .OrderBy(e => e.FullName)
            .ToListAsync(ct);
    }
    public async Task<List<Employee>> GetByDepartmentIdsAsync(
    IEnumerable<int> departmentIds,
    bool trackChanges = false,
    Func<IQueryable<Employee>, IQueryable<Employee>>? include = null,
    CancellationToken ct = default)
    {
        var ids = departmentIds.Distinct().ToList();

        if (!ids.Any())
            return [];

        IQueryable<Employee> query =
            trackChanges ? _dbSet : _dbSet.AsNoTracking();

        query = query.Where(e =>
    e.DepartmentID.HasValue &&
    ids.Contains(e.DepartmentID.Value));

        if (include != null)
            query = include(query);

        return await query
            .OrderBy(e => e.FullName)
            .ToListAsync(ct);
    }

    // ── Pre-delete FK existence checks ───────────────────────────────────────

    public Task<bool> HasLeaveRequestsAsync(int employeeId, CancellationToken ct = default)
        => _context.Set<global::LeaveRequest>()
            .AnyAsync(lr => lr.EmployeeID == employeeId, ct);

    public Task<bool> HasAttendanceRecordsAsync(int employeeId, CancellationToken ct = default)
        => _context.Set<NovaStaff.Models.Entities.AttendanceRecord>()
            .AnyAsync(a => a.EmployeeID == employeeId, ct);

    public Task<bool> HasPayrollDetailsAsync(int employeeId, CancellationToken ct = default)
        => _context.Set<NovaStaff.Models.Entities.PayrollDetail>()
            .AnyAsync(p => p.EmployeeID == employeeId, ct);

    public Task<bool> HasWorkTasksAsync(int employeeId, CancellationToken ct = default)
        => _context.Set<NovaStaff.Models.Entities.WorkTask>()
            .AnyAsync(t => t.EmployeeID == employeeId, ct);

    public async Task<List<Employee>> GetByStatusAsync(
    EmployeeStatus status,
    bool trackChanges = false,
    CancellationToken ct = default)
    {
        var query = trackChanges
            ? _dbSet
            : _dbSet.AsNoTracking();

        return await query
            .Where(e => e.Status == status)
            .OrderBy(e => e.FullName)
            .ToListAsync(ct);
    }

    public Task<int> CountByStatusAsync(
        EmployeeStatus status,
        CancellationToken ct = default)
    {
        return _dbSet.CountAsync(e => e.Status == status, ct);
    }

    public Task<bool> ExistsByStatusAsync(
        EmployeeStatus status,
        CancellationToken ct = default)
    {
        return _dbSet.AnyAsync(e => e.Status == status, ct);
    }

    public async Task<List<Employee>> GetActiveEmployeesAsync(
    bool trackChanges = false,
    CancellationToken ct = default)
    {
        var query = trackChanges
            ? _dbSet
            : _dbSet.AsNoTracking();

        return await query
            .Where(e =>
                e.Status == EmployeeStatus.Active ||
                e.Status == EmployeeStatus.Probation)
            .OrderBy(e => e.FullName)
            .ToListAsync(ct);
    }
}