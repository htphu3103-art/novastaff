// Models/DTOs/Department/DepartmentDescendantQuery.cs
using NovaStaff.Models.Filters;
using System.ComponentModel.DataAnnotations;

namespace NovaStaff.Models.DTOs.Department;

public record DepartmentDescendantQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "PageIndex ph?i >= 1.")]
    public int PageIndex { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize t? 1 ð?n 100.")]
    public int PageSize { get; init; } = 20;

    [MaxLength(100)]
    public string? NameContains { get; init; }

    public bool? IsActive { get; init; }
    public int? ManagerId { get; init; }
    public DepartmentSortField SortBy { get; init; } = DepartmentSortField.OrgNode;
    public bool SortDescending { get; init; } = false;
}



