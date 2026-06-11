using Microsoft.AspNetCore.SignalR;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.Models.DTOs.Attendance;

namespace NovaStaff.API.Hubs;

public class AttendanceNotifier : IAttendanceNotifier
{
    private readonly IHubContext<AttendanceHub> _hub;

    public AttendanceNotifier(IHubContext<AttendanceHub> hub)
    {
        _hub = hub;
    }

    public async Task NotifyCheckInAsync(AttendanceDto dto, CancellationToken ct = default)
    {
        await _hub.Clients.Group("HR")
            .SendAsync("OnCheckIn", dto, ct);
        await _hub.Clients.Group($"user_{dto.EmployeeId}")
            .SendAsync("OnMyCheckIn", dto, ct);
    }

    public async Task NotifyCheckOutAsync(AttendanceDto dto, CancellationToken ct = default)
    {
        await _hub.Clients.Group("HR")
            .SendAsync("OnCheckOut", dto, ct);
        await _hub.Clients.Group($"user_{dto.EmployeeId}")
            .SendAsync("OnMyCheckOut", dto, ct);
    }

    public async Task NotifyRecordUpdatedAsync(AttendanceDto dto, CancellationToken ct = default)
    {
        // HR update record thủ công → notify HR dashboard + nhân viên liên quan
        await _hub.Clients.Group("HR")
            .SendAsync("OnRecordUpdated", dto, ct);
        await _hub.Clients.Group($"user_{dto.EmployeeId}")
            .SendAsync("OnMyRecordUpdated", dto, ct);
    }

    public async Task NotifyRecordDeletedAsync(long recordId, CancellationToken ct = default)
    {
        await _hub.Clients.Group("HR")
            .SendAsync("OnRecordDeleted", recordId, ct);
    }
}