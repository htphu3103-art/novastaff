using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;

namespace NovaStaff.DataLayers.Repositories;

public class LeaveRequestRepository
    : GenericRepository<LeaveRequest, int>, ILeaveRequestRepository
{
    public LeaveRequestRepository(AppDbContext context) : base(context)
    {
    }

    // =========================================================
    // GET BY EMPLOYEE
    // =========================================================
    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(
        int employeeId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(x => x.EmployeeID == employeeId)
            .OrderByDescending(x => x.FromDate)
            .ToListAsync(ct);
    }

    // =========================================================
    // GET PENDING (OPTIONAL FILTER BY DEPARTMENT)
    // =========================================================
    public async Task<IEnumerable<LeaveRequest>> GetPendingAsync(
    int? departmentId = null,
    CancellationToken ct = default)
    {
        IQueryable<LeaveRequest> query = _dbSet
            .AsNoTracking()
            .Include(x => x.Employee)
            .Where(x => x.Status == LeaveRequestStatus.Pending);

        if (departmentId.HasValue)
        {
            query = query.Where(x =>
                x.Employee != null &&
                x.Employee.DepartmentID == departmentId.Value);
        }

        return await query
            .OrderBy(x => x.CreatedDate)
            .ToListAsync(ct);
    }

    // =========================================================
    // COUNT APPROVED DAYS (SUM TotalDays)
    // =========================================================
    public async Task<double> CountApprovedDaysAsync(
        int employeeId,
        int year,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(x =>
                x.EmployeeID == employeeId &&
                x.Status == LeaveRequestStatus.Approved &&
                x.FromDate.Year == year)
            .SumAsync(x => (double?)x.TotalDays, ct) // tránh exception nếu empty
            ?? 0d;
    }
}