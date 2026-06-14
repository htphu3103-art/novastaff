using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Models.DTOs.WorkTasks;

public class WorkTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    // Thông tin người được giao việc
    public int? EmployeeId { get; set; }

    public DateTimeOffset? CompletedDate { get; set; }
    public string? AssigneeName { get; set; }
}
