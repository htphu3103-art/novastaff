using NovaStaff.Models.Common;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;
using System.ComponentModel.DataAnnotations;

public class LeaveRequest : BaseEntity
{
    [Key]
    public int RequestID { get; set; }

    public int? EmployeeID { get; set; }

    public LeaveType LeaveType { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    /// <summary>
    /// Số ngày nghỉ thực tế (đã trừ weekend, holiday, hỗ trợ half-day)
    /// </summary>
    public double TotalDays { get; set; }

    /// <summary>
    /// Có thể mở rộng: nghỉ nửa ngày
    /// </summary>
    public bool IsHalfDayStart { get; set; }

    public bool IsHalfDayEnd { get; set; }

    public string? Reason { get; set; }

    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public virtual Employee? Employee { get; set; }
}