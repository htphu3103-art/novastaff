using System.ComponentModel.DataAnnotations;
using NovaStaff.Models.Enums;

namespace NovaStaff.Models.DTOs.WorkTasks;

public class UpdateWorkTaskRequest
{
    [Required(ErrorMessage = "Tiêu đề công việc không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn độ ưu tiên")]
    public WorkTaskPriority Priority { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    public int? EmployeeId { get; set; }

    // Update cho phép sửa Status, dùng nullable (?) để service check HasValue
    public WorkTaskStatus? Status { get; set; }
}