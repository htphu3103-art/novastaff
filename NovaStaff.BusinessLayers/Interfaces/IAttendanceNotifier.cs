using NovaStaff.Models.DTOs.Attendance;

namespace NovaStaff.BusinessLayers.Interfaces;

public interface IAttendanceNotifier
{
    Task NotifyCheckInAsync(AttendanceDto dto, CancellationToken ct = default);
    Task NotifyCheckOutAsync(AttendanceDto dto, CancellationToken ct = default);
    Task NotifyRecordUpdatedAsync(AttendanceDto dto, CancellationToken ct = default);
    Task NotifyRecordDeletedAsync(long recordId, CancellationToken ct = default);
}