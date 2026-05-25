using NovaStaff.Models.Enums;

namespace NovaStaff.Models.Filters;

public class WorkTaskFilter
{
    /// <summary>
    /// Tìm kiếm gần đúng theo tiêu đề công việc
    /// </summary>
    public string? TitleContains { get; set; }

    /// <summary>
    /// Lọc theo trạng thái (Pending, InProgress, Completed...)
    /// </summary>
    public WorkTaskStatus? Status { get; set; }

    /// <summary>
    /// Lọc theo độ ưu tiên (Low, Medium, High)
    /// </summary>
    public WorkTaskPriority? Priority { get; set; }

    /// <summary>
    /// Lọc các công việc của một nhân viên cụ thể
    /// </summary>
    public int? EmployeeId { get; set; }
}