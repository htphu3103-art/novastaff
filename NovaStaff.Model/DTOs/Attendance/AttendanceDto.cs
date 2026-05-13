// Models/DTOs/Attendance/AttendanceDto.cs
using NovaStaff.Models.Enums;

namespace NovaStaff.Models.DTOs.Attendance;

/// <summary>
/// DTO trả về cho client — read-only, flatten navigation properties.
/// </summary>
public class AttendanceDto
{
    // ── Identity ──────────────────────────────────────────
    public long RecordId { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }

    // ── Attendance Data ───────────────────────────────────
    public DateTime WorkDate { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public decimal? WorkHours { get; set; }         // decimal? — đúng kiểu entity
    public AttendanceStatus Status { get; set; }
    public string StatusDisplay => Status switch     // Không cần toString() ở client
    {
        AttendanceStatus.Present => "Có mặt",
        AttendanceStatus.Late => "Đi trễ",
        AttendanceStatus.Absent => "Vắng mặt",
        AttendanceStatus.HalfDay => "Nửa ngày",
        AttendanceStatus.Leave => "Nghỉ phép",
        _ => "Không xác định"
    };
    public string? Note { get; set; }

    // ── Computed helpers ──────────────────────────────────
    public bool IsCheckedIn => CheckIn.HasValue;
    public bool IsCheckedOut => CheckOut.HasValue;

    // ── Audit (từ BaseEntity) ─────────────────────────────
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public string? ModifiedByName { get; set; }
    public DateTime? ModifiedDate { get; set; }
}