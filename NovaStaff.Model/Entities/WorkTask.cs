using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NovaStaff.Models.Common;
using NovaStaff.Models.Enums;

namespace NovaStaff.Models.Entities;

[Table("WorkTasks")]
public class WorkTask : BaseEntity 
{
    [Key]
    public int TaskID { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    public DateTimeOffset? CompletedDate { get; set; }

    // --- Enums ---
    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Todo;

    public WorkTaskPriority Priority { get; set; } = WorkTaskPriority.Medium;

    // --- Relationships ---
    public int? EmployeeID { get; set; }

    [ForeignKey("EmployeeID")]
    public virtual Employee? Employee { get; set; }

    /* LÝU ?: 
       Toàn b? các trý?ng:
       - CreatedBy, CreatedByName, CreatedDate
       - ModifiedBy, ModifiedByName, ModifiedDate
       - IsDeleted, DeletedBy, DeletedByName, DeletedDate
       Ð?U KHÔNG C?N KHAI BÁO ? ÐÂY n?a v? ð? có trong BaseEntity.
    */
}




