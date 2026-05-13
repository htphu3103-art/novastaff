// Models/Filters/AttendanceFilter.cs
using NovaStaff.Models.Enums;

namespace NovaStaff.Models.Filters;

/// <summary>
/// Filter cho GetPagedAsync — tất cả field đều optional.
///
/// Use cases:
///   HR xem toàn bộ: chỉ set From/To
///   HR xem theo phòng ban: set DepartmentId + From/To  
///   Nhân viên tự xem: set EmployeeId + From/To (hoặc Year/Month)
///   Lọc theo trạng thái: thêm Status
/// </summary>
public class AttendanceFilter
{
    // ── Scope filter ──────────────────────────────────────
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }

    // ── Date range ────────────────────────────────────────
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    // Shorthand: truyền Year+Month thay vì From/To
    // Service sẽ tự convert sang From/To nếu có
    public int? Year { get; set; }
    public int? Month { get; set; }

    // ── Status ────────────────────────────────────────────
    public AttendanceStatus? Status { get; set; }

    // ── Sort ──────────────────────────────────────────────
    public AttendanceSortField SortBy { get; set; } = AttendanceSortField.WorkDate;
    public bool SortDescending { get; set; } = true;
}

public enum AttendanceSortField
{
    WorkDate,
    EmployeeCode,
    WorkHours,
    Status
}