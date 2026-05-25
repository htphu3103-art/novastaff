// Models/Filters/LeaveRequestFilter.cs
using NovaStaff.Models.Enums;

namespace NovaStaff.Models.Filters;

public class LeaveRequestFilter
{
    public int? EmployeeId { get; set; }

    public int? DepartmentId { get; set; }

    public LeaveRequestStatus? Status { get; set; }

    public LeaveType? LeaveType { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public bool SortDescending { get; set; } = true;
}