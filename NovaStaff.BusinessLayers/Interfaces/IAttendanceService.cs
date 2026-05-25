// Services/Interfaces/IAttendanceService.cs
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Attendance;
using NovaStaff.Models.Filters;

namespace NovaStaff.Services.Interfaces;

public interface IAttendanceService
{
    // READ
    Task<AttendanceDto?> GetTodayAsync(int employeeId, CancellationToken ct = default);
    Task<IEnumerable<AttendanceDto>> GetByEmployeeAndMonthAsync(int employeeId, int year, int month, CancellationToken ct = default);
    Task<PagedResult<AttendanceDto>> GetPagedAsync(AttendanceFilter filter, int pageIndex, int pageSize, CancellationToken ct = default);
    Task<double> GetTotalHoursAsync(int employeeId, int year, int month, CancellationToken ct = default);
    Task<double> GetMyTotalHoursAsync(int year, int month, CancellationToken ct = default);

    // ACTIONS
    Task<AttendanceDto> CheckInAsync(int employeeId, CancellationToken ct = default);
    Task<AttendanceDto> CheckOutAsync(int employeeId, CancellationToken ct = default);

    Task<AttendanceDto> CheckInAsync( CancellationToken ct = default);
    Task<AttendanceDto> CheckOutAsync( CancellationToken ct = default);

    // HR MANAGEMENT
    Task<AttendanceDto> CreateManualAsync(CreateAttendanceRequest request, CancellationToken ct = default);
    Task<AttendanceDto> UpdateAsync(long recordId, UpdateAttendanceRequest request, CancellationToken ct = default);
    Task DeleteAsync(long recordId, CancellationToken ct = default);

    Task<AttendanceDto?> GetTodayForCurrentUserAsync(CancellationToken ct = default);
}