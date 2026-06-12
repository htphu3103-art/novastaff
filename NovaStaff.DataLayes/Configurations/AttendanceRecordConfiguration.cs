using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.DataLayers.Helpers;
using NovaStaff.DataLayers.Configurations;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        // -- Base Audit Fields --
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        // -- Bảng & Check Constraints --
        builder.ToTable("AttendanceRecords", t =>
        {
            t.HasCheckConstraint(
                "CK_Attendance_ValidTime",
                "\"CheckIn\" IS NOT NULL OR \"CheckOut\" IS NULL"
            );
            t.HasCheckConstraint(
                "CK_AttendanceRecord_Status",
                EnumConstraintHelper.BuildInConstraint<AttendanceStatus>("Status")
            );
        });

        // -- Primary Key --
        builder.HasKey(a => a.RecordID);
        builder.Property(a => a.RecordID).UseIdentityColumn();

        // -- EmployeeID (FK) --
        builder.Property(a => a.EmployeeID).HasColumnType("int");

        // -- WorkDate --
        builder.Property(a => a.WorkDate)
            .HasColumnType("date")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_DATE");

        // -- CheckIn / CheckOut --
        builder.Property(a => a.CheckIn)
            .HasColumnType("timestamptz");

        builder.Property(a => a.CheckOut)
           .HasColumnType("timestamptz");

        // -- WorkHours (Computed Column for PostgreSQL) --
        builder.Property(a => a.WorkHours)
           .HasPrecision(5, 2)
           .HasComputedColumnSql(
               "CASE WHEN \"CheckIn\" IS NOT NULL AND \"CheckOut\" IS NOT NULL " +
               "THEN (EXTRACT(EPOCH FROM (\"CheckOut\" - \"CheckIn\")) / 3600.0)::numeric(5,2) " +
               "ELSE NULL END",
               stored: true
           );

        builder.Property(a => a.WorkHours)
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        // -- Status --
        builder.Property(a => a.Status)
            .IsRequired()
            .HasDefaultValue(AttendanceStatus.Unknown);

        // -- Note --
        builder.Property(a => a.Note)
            .HasMaxLength(500);

        // -- Relationships --
        builder.HasOne(a => a.Employee)
            .WithMany(e => e.AttendanceRecords)
            .HasForeignKey(a => a.EmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        // -- Indexes --
        builder.HasIndex(a => a.WorkDate)
            .HasDatabaseName("IX_AttendanceRecords_WorkDate");

        builder.HasIndex(a => new { a.EmployeeID, a.WorkDate })
            .IsUnique()
            .HasDatabaseName("IX_AttendanceRecords_EmployeeID_WorkDate");
    }
}
