using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Helpers;
using NovaStaff.Models.Common;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;

namespace NovaStaff.DataLayers.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private static readonly AsyncLocal<bool> _isSavingAudit = new();
    private const string AuditEntriesKey = "AuditInterceptor.TempEntries";

    public AuditInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    // =========================================================
    // NH?P 1: TRÝ?C KHI LÝU (Saving)
    // =========================================================
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (_isSavingAudit.Value) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var context = eventData.Context;
        if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        // 1?? T? ð?ng ði?n CreatedBy/ModifiedBy (Không c?n SoftDelete logic)
        ApplySystemFields(context);

        // 2?? Thu th?p audit data
        var entries = OnBeforeSaveChanges(context);
        if (entries.Any())
        {
            var items = AppDbContext.DbContextStorage.GetOrCreate(context);
            items[AuditEntriesKey] = entries;
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // =========================================================
    // NH?P 2: SAU KHI LÝU (Saved)
    // =========================================================
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null || _isSavingAudit.Value) return result;

        var items = AppDbContext.DbContextStorage.GetOrCreate(context);
        if (!items.TryGetValue(AuditEntriesKey, out var tempEntriesObj)) return result;

        var entries = (List<AuditEntry>)tempEntriesObj;
        if (entries?.Any() != true) return result;

        _isSavingAudit.Value = true;
        try
        {
            UpdateTemporaryValues(entries);

            var logs = entries
                .Where(e => e.Action != AuditAction.Unknown)
                .Select(e => e.ToAuditLog())
                .ToList();

            if (logs.Any())
            {
                context.Set<AuditLog>().AddRange(logs);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuditInterceptor] Save failed: {ex.Message}");
        }
        finally
        {
            _isSavingAudit.Value = false;
            items.Remove(AuditEntriesKey);
        }

        return result;
    }

    // =========================================================
    // HELPER METHODS
    // =========================================================

    private void ApplySystemFields(DbContext context)
    {
        var userId = _currentUserService.GetUserId();
        var userName = _currentUserService.GetDisplayName();

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:

                    entry.Entity.CreatedDate = DateTime.UtcNow;

                    Console.WriteLine("=== BEFORE SAVE ===");

                    foreach (var p in entry.Properties)
                    {
                        if (p.CurrentValue is DateTime dt)
                        {
                            Console.WriteLine(
                                $"{p.Metadata.Name} => {dt.Kind} => {dt}");
                        }
                    }

                    break;
            }
        }
    }

    private List<AuditEntry> OnBeforeSaveChanges(DbContext context)
    {
        var userId = _currentUserService.GetUserId()?.ToString();
        var ip = _currentUserService?.GetIpAddress();
        var agent = _currentUserService?.GetUserAgent();
        var entries = new List<AuditEntry>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog ||
                entry.State is EntityState.Detached or EntityState.Unchanged ||
                string.IsNullOrEmpty(entry.Metadata.GetTableName()))
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                TableName = entry.Metadata.GetTableName()!,
                ChangedBy = userId,
                IPAddress = ip,
                UserAgent = agent,
                Action = GetAuditAction(entry)
            };
            entries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                var propName = property.Metadata.Name;

                if (IsSystemField(propName)) continue;

                if (property.IsTemporary)
                {
                    auditEntry.TemporaryProperties.Add(property);
                    continue;
                }

                // ?? FIX 1: PrimaryKey - Lýu c? Current & Original (quan tr?ng cho DELETE)
                if (property.Metadata.IsPrimaryKey())
                {
                    var keyValue = property.CurrentValue ?? property.OriginalValue;
                    if (keyValue != null)
                        auditEntry.KeyValues[propName] = keyValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added when property.CurrentValue != null:
                        auditEntry.NewValues[propName] = property.CurrentValue;
                        break;

                    // ?? FIX 2: HARDCODE DELETE - Lýu FULL DATA vào OldValues
                    case EntityState.Deleted:
                        // Ýu tiên OriginalValue (d? li?u DB), fallback CurrentValue
                        var deleteValue = property.OriginalValue ?? property.CurrentValue;
                        if (deleteValue != null)
                            auditEntry.OldValues[propName] = deleteValue;
                        break;

                    case EntityState.Modified when property.IsModified:
                        // ?? FIX 3: Modified - Ki?m tra null c?n th?n hõn
                        if (property.OriginalValue != null && property.CurrentValue != null)
                        {
                            auditEntry.OldValues[propName] = property.OriginalValue;
                            auditEntry.NewValues[propName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }
        return entries;
    }

    private static void UpdateTemporaryValues(List<AuditEntry> entries)
    {
        foreach (var entry in entries.Where(e => e.Entry != null))
        {
            foreach (var prop in entry.TemporaryProperties)
            {
                if (prop.CurrentValue == null) continue;

                if (prop.Metadata.IsPrimaryKey())
                    entry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                else
                    entry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
            }
        }
    }

    private static AuditAction GetAuditAction(EntityEntry entry)
    {
        return entry.State switch
        {
            EntityState.Added => AuditAction.Insert,
            EntityState.Deleted => AuditAction.Delete,
            EntityState.Modified => AuditAction.Update,
            _ => AuditAction.Unknown
        };
    }

    private static bool IsSystemField(string propertyName) =>
        propertyName is "CreatedDate" or "CreatedBy" or "CreatedByName" or
        "ModifiedDate" or "ModifiedBy" or "ModifiedByName";
}



