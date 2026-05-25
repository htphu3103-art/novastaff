// Models/DTOs/LeaveRequest/LeaveRequestDto.cs
using NovaStaff.Models.Enums;

namespace NovaStaff.Models.DTOs.LeaveRequest;

public class LeaveRequestDto
{
    public int RequestId { get; set; }

    public int? EmployeeId { get; set; }

    public string? EmployeeCode { get; set; }

    public string? EmployeeName { get; set; }

    public LeaveType LeaveType { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public double TotalDays { get; set; }

    public bool IsHalfDayStart { get; set; }

    public bool IsHalfDayEnd { get; set; }

    public string? Reason { get; set; }

    public LeaveRequestStatus Status { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public DateTime CreatedDate { get; set; }
}