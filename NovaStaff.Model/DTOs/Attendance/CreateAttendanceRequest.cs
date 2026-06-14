// Models/DTOs/Attendance/CreateAttendanceRequest.cs
using System.ComponentModel.DataAnnotations;
using NovaStaff.Models.Enums;

namespace NovaStaff.Models.DTOs.Attendance;

/// <summary>
/// HR tạo record thủ công: bù công, điều chỉnh lịch sử.
/// CheckIn/CheckOut nullable vì có thể tạo record Absent/Leave không có giờ.
/// </summary>
public class CreateAttendanceRequest
{
    [Required(ErrorMessage = "EmployeeId không được trống")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "WorkDate không được trống")]
    public DateOnly WorkDate { get; set; }

    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    public string? Note { get; set; }
}