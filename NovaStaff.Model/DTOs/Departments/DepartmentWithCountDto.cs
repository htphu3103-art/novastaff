namespace NovaStaff.BusinessLayers.DTOs.Departments;

public sealed record DepartmentWithCountDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int EmployeeCount { get; init; }
}



