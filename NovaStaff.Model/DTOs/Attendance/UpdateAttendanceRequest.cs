// Models/DTOs/Attendance/UpdateAttendanceRequest.cs
using System.ComponentModel.DataAnnotations;
using NovaStaff.Models.Enums;

namespace NovaStaff.Models.DTOs.Attendance;

/// <summary>
/// HR chỉnh sửa record đã tồn tại.
/// Không cho đổi EmployeeId / WorkDate sau khi tạo — tránh logic phức tạp.
/// </summary>
public class UpdateAttendanceRequest
{
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }

    [Required]
    public AttendanceStatus Status { get; set; }

    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    public string? Note { get; set; }
}