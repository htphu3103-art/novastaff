using NovaStaff.Models.Common;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;
namespace NovaStaff.Models.Entities;

public class Employee : BaseEntity
{
    public int EmployeeID { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public GenderType Gender { get; set; } = GenderType.Other;
    public DateTime? BirthDate { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public int? DepartmentID { get; set; }

    // --- THÊM QU?N L? TR?C TI?P ---
    public int? SupervisorID { get; set; }
    public virtual Employee? Supervisor { get; set; }
    // ------------------------------

    public string? Position { get; set; }
    public int? JobLevel { get; set; }
    public decimal BaseSalary { get; set; }
    public DateTime? JoinDate { get; set; }
    public string? ContractType { get; set; }
    public EmployeeStatus Status { get; set; }

    // Navigation
    public Department? Department { get; set; }
    public virtual User? User { get; set; }

    // Thu?c tính ð? l?y danh sách c?p dý?i (n?u c?n)
    public virtual ICollection<Employee> Subordinates { get; set; } = [];

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
    public ICollection<PayrollDetail> PayrollDetails { get; set; } = [];
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
    public ICollection<WorkTask> WorkTasks { get; set; } = [];
}




