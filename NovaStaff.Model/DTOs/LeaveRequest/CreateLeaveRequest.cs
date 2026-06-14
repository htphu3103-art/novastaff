using NovaStaff.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace NovaStaff.Models.DTOs.LeaveRequest;

public class CreateLeaveRequest
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public LeaveType LeaveType { get; set; }

    [Required]
    public DateOnly FromDate { get; set; }

    [Required]
    public DateOnly ToDate { get; set; }

    public bool IsHalfDayStart { get; set; }

    public bool IsHalfDayEnd { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}