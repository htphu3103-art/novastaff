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
            // 1. Khai báo các ràng bu?c Enum
            var actionConstraint = EnumConstraintHelper.BuildInConstraint<AuditAction>(nameof(AuditLog.Action));
            const string tinyint = "tinyint";

            // 2. C?u h?nh b?ng
            builder.ToTable("AuditLog", b =>
            {
                b.HasCheckConstraint("CK_AuditLog_Action", actionConstraint);
            });

            // 3. Khóa chính
            builder.HasKey(a => a.AuditID);
            builder.Property(a => a.AuditID).UseIdentityColumn();

            // 4. C?u h?nh các c?t
            builder.Property(a => a.TableName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)"); // ? Thêm explicit type cho nh?t quán

            builder.Property(a => a.Action)
                .IsRequired()
                .HasColumnType(tinyint)
                .HasDefaultValue(AuditAction.Unknown);

            builder.Property(a => a.RecordID)
                .HasMaxLength(50)                // ? Gi?i h?n l?i — ID không c?n nvarchar(max)
                .HasColumnType("nvarchar(50)");

            builder.Property(a => a.ChangedBy)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)"); // ? Thêm explicit type

            builder.Property(a => a.ChangedDate)
                .IsRequired()
                .HasColumnType("datetime2")      // ? Thêm explicit type
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(a => a.IPAddress)
                .HasMaxLength(50)
                .IsUnicode(false)                // ? IP ch? là ASCII, dùng varchar ti?t ki?m hõn
                .HasColumnType("varchar(50)");

            builder.Property(a => a.UserAgent)
                .HasMaxLength(500)               // ? Gi?i h?n l?i — không c?n nvarchar(max)
                .HasColumnType("nvarchar(500)");

            // OldData, NewData gi? nvarchar(max) v? JSON có th? r?t dài — ðúng r?i

            // 5. Indexes
            builder.HasIndex(a => new { a.TableName, a.ChangedDate })
                .HasDatabaseName("IX_AuditLog_Table_Date");

            builder.HasIndex(a => a.ChangedBy)
                .HasDatabaseName("IX_AuditLog_ChangedBy");

            builder.HasIndex(a => a.RecordID)
                .HasDatabaseName("IX_AuditLog_RecordID");
        }
    }



