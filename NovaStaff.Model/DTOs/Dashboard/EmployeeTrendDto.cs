namespace NovaStaff.Models.DTOs.Dashboard;

public sealed class EmployeeTrendDto
{
    public string Month { get; init; } = null!;
    public int Year { get; init; }
    public int NewEmployees { get; init; }
    public int LeftEmployees { get; init; }
    public double TaskCompletionRate { get; init; }
}