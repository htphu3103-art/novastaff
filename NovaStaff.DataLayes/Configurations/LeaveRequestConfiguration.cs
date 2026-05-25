using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.DataLayers.Helpers;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        // -- Base Audit Fields --
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        // -- Bảng & Check Constraints --
        builder.ToTable("LeaveRequests", b =>
        {
            b.HasCheckConstraint(
                "CK_LeaveRequest_DateRange",
                "\"ToDate\" >= \"FromDate\""
            );
            b.HasCheckConstraint(
                "CK_LeaveRequest_ApprovedDate",
                "\"ApprovedDate\" IS NULL OR \"Status\" <> 1"
            );

            b.HasCheckConstraint(
                "CK_LeaveRequest_ApprovedBy",
                "\"ApprovedBy\" IS NULL OR \"Status\" <> 1"
            );
            b.HasCheckConstraint(
                "CK_LeaveRequest_Status",
                EnumConstraintHelper.BuildInConstraint<LeaveRequestStatus>("Status")
            );
            b.HasCheckConstraint(
                "CK_LeaveRequest_LeaveType",
                EnumConstraintHelper.BuildInConstraint<LeaveType>("LeaveType")
            );
        });

        // -- Primary Key --
        builder.HasKey(l => l.RequestID);
        builder.Property(l => l.RequestID).UseIdentityColumn();

        // -- EmployeeID (FK) --
        builder.Property(l => l.EmployeeID).HasColumnType("int");

        // -- LeaveType --
        builder.Property(l => l.LeaveType)
            .IsRequired()
            .HasDefaultValue(LeaveType.Unknown);

        // -- FromDate / ToDate --
        builder.Property(l => l.FromDate)
            .IsRequired();

        builder.Property(l => l.ToDate)
            .IsRequired();

        // -- Reason --
        builder.Property(l => l.Reason)
            .HasMaxLength(500);

        // -- Status --
        builder.Property(l => l.Status)
            .IsRequired()
            .HasDefaultValue(LeaveRequestStatus.Pending)
            .HasSentinel(LeaveRequestStatus.Unknown);

        // -- ApprovedBy / ApprovedDate --
        builder.Property(l => l.ApprovedBy).HasColumnType("int");
        builder.Property(l => l.ApprovedDate);


        // -- Relationship --
        builder.HasOne(l => l.Employee)
            .WithMany(e => e.LeaveRequests)
            .HasForeignKey(l => l.EmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        // -- Indexes --
        builder.HasIndex(l => new { l.EmployeeID, l.FromDate })
            .HasDatabaseName("IX_LeaveRequests_Employee_Date");

        builder.HasIndex(l => l.Status)
            .HasDatabaseName("IX_LeaveRequests_Status");
    }
}
