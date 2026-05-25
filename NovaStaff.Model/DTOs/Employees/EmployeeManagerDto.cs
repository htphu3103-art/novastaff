namespace NovaStaff.Models.DTOs.Employees;

public class EmployeeManagerDto
{
    public int EmployeeID { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Position { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
}