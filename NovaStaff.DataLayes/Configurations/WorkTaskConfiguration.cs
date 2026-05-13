using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.DataLayers.Helpers;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        // ?? B?ng & Check Constraints ???????????????????????????????????????????
        builder.ToTable("WorkTasks", b =>
        {
            b.HasCheckConstraint(
                "CK_WorkTask_Status",
                EnumConstraintHelper.BuildInConstraint<WorkTaskStatus>("Status")
            );
            b.HasCheckConstraint(
                "CK_WorkTask_Priority",
                EnumConstraintHelper.BuildInConstraint<WorkTaskPriority>("Priority")
            );
        });

        // ?? Primary Key ????????????????????????????????????????????????????????
        builder.HasKey(t => t.TaskID);
        builder.Property(t => t.TaskID).UseIdentityColumn();

        // ?? Properties ?????????????????????????????????????????????????????????
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("nvarchar(255)");

        builder.Property(t => t.Description)
            .HasMaxLength(2000)
            .HasColumnType("nvarchar(2000)");

        builder.Property(t => t.Status)
            .IsRequired()
            .HasColumnType("tinyint")
            .HasDefaultValue(WorkTaskStatus.Todo)
            .HasSentinel(WorkTaskStatus.Unknown);

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasColumnType("tinyint")
            .HasDefaultValue(WorkTaskPriority.Medium)
            .HasSentinel(WorkTaskPriority.Unknown);

        builder.Property(t => t.DueDate)
            .HasColumnType("datetime2");

        builder.Property(t => t.CompletedDate)
            .HasColumnType("datetime2");

        // ?? Audit Fields (C?p nh?t theo BaseEntity) ???????????????????????????
        // Ð?i CreatedAt -> CreatedDate
        builder.Property(t => t.CreatedDate)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        // CreatedBy nay là int? (theo BaseEntity)
        builder.Property(t => t.CreatedBy);

        builder.Property(t => t.CreatedByName)
            .HasMaxLength(100)
            .HasColumnType("nvarchar(100)");

        builder.Property(t => t.ModifiedDate)
            .HasColumnType("datetime2");

        builder.Property(t => t.ModifiedBy);

        builder.Property(t => t.ModifiedByName)
            .HasMaxLength(100)
            .HasColumnType("nvarchar(100)");

        // ?? Soft Delete (Ð? có trong BaseEntity) ??????????????????????????????
        

        // ?? Relationships ??????????????????????????????????????????????????????
        builder.HasOne(t => t.Employee)
            .WithMany(e => e.WorkTasks)
            .HasForeignKey(t => t.EmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        // ?? Indexes ????????????????????????????????????????????????????????????
        builder.HasIndex(t => new { t.EmployeeID, t.Status })
            .HasDatabaseName("IX_WorkTasks_Employee_Status");

        builder.HasIndex(t => t.DueDate)
        .HasFilter("DueDate IS NOT NULL")
        .HasDatabaseName("IX_WorkTasks_DueDate");

        // Lýu ?: Không c?n builder.HasQueryFilter ? ðây v? AppDbContext ð? áp d?ng Global Filter r?i
    }
}



