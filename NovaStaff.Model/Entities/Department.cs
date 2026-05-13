using Microsoft.EntityFrameworkCore;
using NovaStaff.Models.Common;
using NovaStaff.Models.Entities;
public class Department : BaseEntity
{
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Code { get; set; }

    private HierarchyId _orgNode = HierarchyId.GetRoot();
    public HierarchyId OrgNode
    {
        get => _orgNode;
        set => _orgNode = value; // ? b? OrgLevel = ... ? ðây
    }

    public short? OrgLevel { get; private set; }

    public int? ManagerEmployeeID { get; set; }
    public virtual Employee? Manager { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Employee> Employees { get; set; } = [];
}




