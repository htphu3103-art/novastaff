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
    private readonly ITimeZoneProvider _timeZoneProvider;
    public AttendanceRepository(AppDbContext context,
                                IDateTimeService dateTimeService,
                                ITimeZoneProvider timeZoneProvider)
        : base(context)
    {
        _dateTimeService = dateTimeService;
        _timeZoneProvider = timeZoneProvider;
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
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1);

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
        var today = _timeZoneProvider.TodayLocal;
        var tomorrow = today.AddDays(1);

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(a =>
                a.EmployeeID == employeeId &&
                a.WorkDate >= today &&
                a.WorkDate < tomorrow,
                ct);
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
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1);

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