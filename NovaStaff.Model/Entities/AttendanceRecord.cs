using Microsoft.EntityFrameworkCore;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Common;  


namespace NovaStaff.Models.Entities;

public class AttendanceRecord : BaseEntity 
{
    public long RecordID { get; set; }
    public int? EmployeeID { get; set; }
    public DateTime WorkDate { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public decimal? WorkHours { get; private set; } 
    public AttendanceStatus Status { get; set; }
    public string? Note { get; set; }

    public virtual Employee? Employee { get; set; }
}




