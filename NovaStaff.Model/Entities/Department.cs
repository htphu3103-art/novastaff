using Microsoft.EntityFrameworkCore;
using NovaStaff.Models.Common;
using NovaStaff.Models.Entities;

public class Department : BaseEntity
{
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Code { get; set; }

    private string _orgPath = "/";
    public string OrgPath
    {
        get => _orgPath;
        set
        {
            _orgPath = value;
            OrgLevel = (short)(string.IsNullOrEmpty(value) || value == "/"
                ? 0
                : value.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries).Length);
        }
    }

    public short? OrgLevel { get; set; }

    public int? ManagerEmployeeID { get; set; }
    public virtual Employee? Manager { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Employee> Employees { get; set; } = [];
}
