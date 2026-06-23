namespace NovaStaff.Models.DTOs.Dashboard;

public sealed class KpiSummaryDto
{
    public int TotalEmployees { get; init; }
    public AttendanceSummary Attendance { get; init; } = null!;
    public int PendingRequests { get; init; }
    public NewHiresSummary NewHires { get; init; } = null!;
}

public sealed class AttendanceSummary
{
    public int PresentToday { get; init; }
    public int AbsentToday { get; init; }
    public int AbsentWithLeave { get; init; }
    public int AbsentWithoutLeave { get; init; }
    public double AttendanceRate { get; init; }
}

public sealed class NewHiresSummary
{
    public int ThisMonth { get; init; }
    public int LastMonth { get; init; }
    public double GrowthRatePercent { get; init; }
}