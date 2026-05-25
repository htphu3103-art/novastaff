// Models/DTOs/LeaveRequest/ApproveLeaveRequest.cs
namespace NovaStaff.Models.DTOs.LeaveRequest;

public class ApproveLeaveRequest
{
    public bool IsApproved { get; set; }

    public string? Note { get; set; }
}