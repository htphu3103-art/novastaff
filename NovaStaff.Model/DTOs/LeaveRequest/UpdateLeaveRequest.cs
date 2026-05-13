// Models/DTOs/LeaveRequest/UpdateLeaveRequest.cs
using NovaStaff.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace NovaStaff.Models.DTOs.LeaveRequest;

public class UpdateLeaveRequest
{
    [Required]
    public LeaveType LeaveType { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    public bool IsHalfDayStart { get; set; }

    public bool IsHalfDayEnd { get; set; }

    public string? Reason { get; set; }
}