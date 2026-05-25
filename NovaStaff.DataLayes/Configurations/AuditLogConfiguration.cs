using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.DataLayers.Helpers;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // 1. Khai báo các ràng buộc Enum
        var actionConstraint = EnumConstraintHelper.BuildInConstraint<AuditAction>(nameof(AuditLog.Action));

        // 2. Cấu hình bảng
        builder.ToTable("AuditLog", b =>
        {
            b.HasCheckConstraint("CK_AuditLog_Action", actionConstraint);
        });

        // 3. Khóa chính
        builder.HasKey(a => a.AuditID);
        builder.Property(a => a.AuditID).UseIdentityColumn();

        // 4. Cấu hình các cột
        builder.Property(a => a.TableName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasColumnType("smallint")
            .HasDefaultValue(AuditAction.Unknown);

        builder.Property(a => a.RecordID)
            .HasMaxLength(50);

        builder.Property(a => a.ChangedBy)
            .HasMaxLength(100);

        builder.Property(a => a.ChangedDate)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(a => a.IPAddress)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnType("varchar(50)");

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);

        // 5. Indexes
        builder.HasIndex(a => new { a.TableName, a.ChangedDate })
            .HasDatabaseName("IX_AuditLog_Table_Date");

        builder.HasIndex(a => a.ChangedBy)
            .HasDatabaseName("IX_AuditLog_ChangedBy");

        builder.HasIndex(a => a.RecordID)
            .HasDatabaseName("IX_AuditLog_RecordID");
    }
}
