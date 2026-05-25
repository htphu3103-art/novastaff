// Models/Filters/DepartmentDescendantFilter.cs
namespace NovaStaff.Models.Filters;

public sealed class DepartmentDescendantFilter
{
    public string? NameContains { get; init; }
    public bool? IsActive { get; init; }
    public int? ManagerId { get; init; }

    public DepartmentSortField SortBy { get; init; } = DepartmentSortField.OrgNode;
    public bool SortDescending { get; init; } = false;
}

public enum DepartmentSortField
{
    OrgNode,   // default — theo th? t? cây
    Name,
    CreatedAt
}



