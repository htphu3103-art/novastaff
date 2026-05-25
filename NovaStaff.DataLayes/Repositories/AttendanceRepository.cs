// DataLayers/Repositories/AttendanceRepository.cs
using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Repositories;

public class AttendanceRepository
    : GenericRepository<AttendanceRecord, long>, IAttendanceRepository
{
    private readonly IDateTimeService _dateTimeService;

    public AttendanceRepository(AppDbContext context, IDateTimeService dateTimeService)
        : base(context)
    {
        _dateTimeService = dateTimeService;
    }

    // =========================================================
    // GetByEmployeeAndMonthAsync
    // =========================================================

    public async Task<IEnumerable<AttendanceRecord>> GetByEmployeeAndMonthAsync(
        int employeeId,
        int year,
        int month,
        CancellationToken ct = default)
    {
        // Tính khoảng ngày để EF sinh ra range query thay vì YEAR()/MONTH()
        // => index-friendly, tránh function scan
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1); // exclusive upper bound

        return await _dbSet
            .AsNoTracking()
            .Where(a => a.EmployeeID == employeeId
                     && a.WorkDate >= from
                     && a.WorkDate < to)
            .OrderBy(a => a.WorkDate)
            .ToListAsync(ct);
    }

    // =========================================================
    // GetTodayAsync
    // =========================================================

    public async Task<AttendanceRecord?> GetTodayAsync(
        int employeeId,
        CancellationToken ct = default)
    {
        // Dùng LocalNow (giờ VN) tránh edge case timezone
        var today = _dateTimeService.LocalNow.Date;
        var tomorrow = today.AddDays(1);

        return await _dbSet
            .AsNoTracking()
            .Where(a => a.EmployeeID == employeeId
                     && a.WorkDate >= today
                     && a.WorkDate < tomorrow)
            .FirstOrDefaultAsync(ct);
    }

    // =========================================================
    // GetTotalHoursAsync
    // =========================================================

    public async Task<double> GetTotalHoursAsync(
        int employeeId,
        int year,
        int month,
        CancellationToken ct = default)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1);

        // SUM trả về null nếu không có row nào → ?? 0 để tránh NullReferenceException
        var total = await _dbSet
            .AsNoTracking()
            .Where(a => a.EmployeeID == employeeId
                     && a.WorkDate >= from
                     && a.WorkDate < to
                     && a.WorkHours != null)
            .SumAsync(a => (double?)a.WorkHours, ct);

        return total ?? 0d;
    }
}