using NovaStaff.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovaStaff.Models.Entities;

[Table("AuditLog")]
public class AuditLog
{
    [Key]
    public long AuditID { get; set; }

    [MaxLength(100)]
    public string TableName { get; set; } = string.Empty;

    public AuditAction Action { get; set; } = AuditAction.Unknown;

    public string? RecordID { get; set; }

    public string? OldData { get; set; } // D? li?u trý?c khi s?a (JSON)

    public string? NewData { get; set; } // D? li?u sau khi s?a (JSON)

    [MaxLength(100)]
    public string? ChangedBy { get; set; }

    public DateTimeOffset ChangedDate { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(50)]
    public string? IPAddress { get; set; }

    public string? UserAgent { get; set; }


}




