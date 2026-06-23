using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.DTOs.Dashboard;
using NovaStaff.Models.Enums;

namespace NovaStaff.DataLayers.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;
    private readonly ITimeZoneProvider _tz; 

    public DashboardRepository(AppDbContext context, ITimeZoneProvider tz) 
    {
        _context = context;
        _tz = tz; 
    }

    public async Task<KpiSummaryDto> GetKpiSummaryAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var thisMonthStart = new DateOnly(today.Year, today.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);
        var lastMonthEnd = thisMonthStart.AddDays(-1);

        // ── Chạy tuần tự thay vì Task.WhenAll ────────────────────────────
        var totalEmployees = await _context.Employees
            .CountAsync(e =>
                e.Status == EmployeeStatus.Active ||
                e.Status == EmployeeStatus.Probation, ct);

        var presentToday = await _context.AttendanceRecords
            .CountAsync(a =>
                a.WorkDate == today &&
                (a.Status == AttendanceStatus.Present ||
                 a.Status == AttendanceStatus.Late), ct);

        var absentWithLeave = await _context.AttendanceRecords
            .CountAsync(a =>
                a.WorkDate == today &&
                a.Status == AttendanceStatus.Leave, ct);

        var absentWithoutLeave = await _context.AttendanceRecords
            .CountAsync(a =>
                a.WorkDate == today &&
                a.Status == AttendanceStatus.Absent, ct);

        var pendingRequests = await _context.LeaveRequests
            .CountAsync(lr => lr.Status == LeaveRequestStatus.Pending, ct);

        var thisMonth = await _context.Employees
            .CountAsync(e => e.JoinDate >= thisMonthStart && e.JoinDate <= today, ct);

        var lastMonth = await _context.Employees
            .CountAsync(e => e.JoinDate >= lastMonthStart && e.JoinDate <= lastMonthEnd, ct);

        // ── Tính toán ─────────────────────────────────────────────────────
        var absentToday = absentWithLeave + absentWithoutLeave;
        var attendanceRate = totalEmployees == 0
            ? 0
            : Math.Round((double)presentToday / totalEmployees * 100, 1);
        var growthRate = lastMonth == 0
            ? 0
            : Math.Round((double)(thisMonth - lastMonth) / lastMonth * 100, 1);

        return new KpiSummaryDto
        {
            TotalEmployees = totalEmployees,
            PendingRequests = pendingRequests,
            Attendance = new AttendanceSummary
            {
                PresentToday = presentToday,
                AbsentToday = absentToday,
                AbsentWithLeave = absentWithLeave,
                AbsentWithoutLeave = absentWithoutLeave,
                AttendanceRate = attendanceRate
            },
            NewHires = new NewHiresSummary
            {
                ThisMonth = thisMonth,
                LastMonth = lastMonth,
                GrowthRatePercent = growthRate
            }
        };
    }

    public async Task<List<EmployeeTrendDto>> GetEmployeeTrendsAsync(
    int limit,
    CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var months = Enumerable.Range(0, limit)
            .Select(i => today.AddMonths(-(limit - 1 - i)))
            .Select(d => new DateOnly(d.Year, d.Month, 1))
            .ToList();

        var fromDate = months.First();
        var toDate = new DateOnly(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
        var fromDto = new DateTimeOffset(fromDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toDto = new DateTimeOffset(toDate.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        // ── Chạy tuần tự ─────────────────────────────────────────────────
        var newHires = await _context.Employees
            .Where(e => e.JoinDate >= fromDate && e.JoinDate <= toDate)
            .GroupBy(e => new { e.JoinDate!.Value.Year, e.JoinDate.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(ct);

        var left = await _context.Employees
            .Where(e => e.TerminationDate >= fromDate && e.TerminationDate <= toDate)
            .GroupBy(e => new { e.TerminationDate!.Value.Year, e.TerminationDate.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(ct);

        var totalTasks = await _context.WorkTasks
            .Where(t => t.DueDate.HasValue &&
                        t.DueDate.Value >= fromDto &&
                        t.DueDate.Value <= toDto)
            .GroupBy(t => new { t.DueDate!.Value.Year, t.DueDate.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Count() })
            .ToListAsync(ct);

        var doneTasks = await _context.WorkTasks
            .Where(t => t.Status == WorkTaskStatus.Done &&
                        t.DueDate.HasValue &&
                        t.CompletedDate.HasValue &&
                        t.CompletedDate <= t.DueDate &&
                        t.DueDate.Value >= fromDto &&
                        t.DueDate.Value <= toDto)
            .GroupBy(t => new { t.DueDate!.Value.Year, t.DueDate.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Done = g.Count() })
            .ToListAsync(ct);

        // ── Build map ─────────────────────────────────────────────────────
        var newHiresMap = newHires.ToDictionary(x => new DateOnly(x.Year, x.Month, 1), x => x.Count);
        var leftMap = left.ToDictionary(x => new DateOnly(x.Year, x.Month, 1), x => x.Count);
        var totalTasksMap = totalTasks.ToDictionary(x => new DateOnly(x.Year, x.Month, 1), x => x.Total);
        var doneTasksMap = doneTasks.ToDictionary(x => new DateOnly(x.Year, x.Month, 1), x => x.Done);

        return months.Select(m =>
        {
            var total = totalTasksMap.GetValueOrDefault(m, 0);
            var done = doneTasksMap.GetValueOrDefault(m, 0);
            var rate = total == 0 ? 0 : Math.Round((double)done / total * 100, 1);

            return new EmployeeTrendDto
            {
                Month = m.ToString("MMM"),
                Year = m.Year,
                NewEmployees = newHiresMap.GetValueOrDefault(m, 0),
                LeftEmployees = leftMap.GetValueOrDefault(m, 0),
                TaskCompletionRate = rate
            };
        }).ToList();
    }
}