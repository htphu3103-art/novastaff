using Microsoft.EntityFrameworkCore.ChangeTracking;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;
using NovaStaff.Shared.Serialization;
using System.Text.Json;

namespace NovaStaff.DataLayers.Helpers;

public class AuditEntry
{
    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
    }

    public EntityEntry Entry { get; }

    public string TableName { get; set; } = string.Empty;

    public string? ChangedBy { get; set; }

    public string? IPAddress { get; set; }

    public string? UserAgent { get; set; }

    public AuditAction Action { get; set; }

    public Dictionary<string, object> KeyValues { get; } = new();

    public Dictionary<string, object> OldValues { get; } = new();

    public Dictionary<string, object> NewValues { get; } = new();

    public List<PropertyEntry> TemporaryProperties { get; } = new();

    public bool HasTemporaryProperties =>
        TemporaryProperties.Any();

    public AuditLog ToAuditLog()
    {
        var log = new AuditLog
        {
            TableName = TableName,
            Action = Action,
            ChangedBy = ChangedBy ?? "System",
            ChangedDate = DateTime.UtcNow,
            IPAddress = IPAddress,
            UserAgent = UserAgent,

            OldData = OldValues.Count == 0
                ? null
                : JsonSerializer.Serialize(
                    OldValues,
                    SystemJson.Default),

            NewData = NewValues.Count == 0
                ? null
                : JsonSerializer.Serialize(
                    NewValues,
                    SystemJson.Default)
        };

        if (KeyValues.Any())
        {
            var keyParts = KeyValues.Values
                .Where(v => v != null)
                .Select(v => v.ToString())
                .Where(s => !string.IsNullOrEmpty(s))
                .Take(3)
                .ToList();

            log.RecordID = string.Join("|", keyParts);
        }
        else
        {
            var keys = Entry.Properties
                .Where(p => p.Metadata.IsPrimaryKey())
                .Select(p =>
                    p.CurrentValue?.ToString()
                    ?? p.OriginalValue?.ToString())
                .Where(id => !string.IsNullOrEmpty(id))
                .Take(3)
                .ToList();

            log.RecordID = string.Join("|", keys);
        }

        return log;
    }
}