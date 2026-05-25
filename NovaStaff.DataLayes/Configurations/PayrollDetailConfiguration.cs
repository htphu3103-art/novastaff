using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.DataLayers.Helpers;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class PayrollDetailConfiguration : IEntityTypeConfiguration<PayrollDetail>
{
    public void Configure(EntityTypeBuilder<PayrollDetail> builder)
    {
        // -- Base Audit Fields --
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        // -- Bảng & Check Constraints --
        builder.ToTable("PayrollDetails", b =>
        {
            b.HasCheckConstraint("CK_PayrollDetail_NetSalary", "\"NetSalary\" >= 0");
            b.HasCheckConstraint("CK_PayrollDetail_TotalIncome", "\"TotalIncome\" >= 0");
            b.HasCheckConstraint("CK_PayrollDetail_WorkDays", "\"ActualWorkDays\" >= 0");
            b.HasCheckConstraint("CK_PayrollDetail_PaidDate",
                                $"\"PaidDate\" IS NULL OR \"Status\" = {(byte)PayrollStatus.Paid}"
                            );
            b.HasCheckConstraint(
                "CK_PayrollDetail_Status",
                EnumConstraintHelper.BuildInConstraint<PayrollStatus>("Status")
            );
        });

        // -- Primary Key --
        builder.HasKey(p => p.DetailID);
        builder.Property(p => p.DetailID).UseIdentityColumn();

        // -- Foreign Keys --
        builder.Property(p => p.PeriodID).HasColumnType("int");
        builder.Property(p => p.EmployeeID).HasColumnType("int");

        // -- Money Fields --
        builder.Property(p => p.BaseSalarySnapshot)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.TotalIncome)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.NetSalary)
            .IsRequired()
            .HasPrecision(18, 2);

        // -- Work Days --
        builder.Property(p => p.ActualWorkDays)
            .IsRequired()
            .HasPrecision(4, 1);

        // -- JSON Columns --
        builder.Property(p => p.BonusAndAllowancesJson);

        builder.Property(p => p.DeductionsJson);

        // -- Status --
        builder.Property(p => p.Status)
            .IsRequired()
            .HasDefaultValue(PayrollStatus.Draft)
            .HasSentinel(PayrollStatus.Unknown);

        // -- PaidDate --
        builder.Property(p => p.PaidDate);

        // -- Relationships --
        builder.HasOne(p => p.Period)
            .WithMany(pp => pp.PayrollDetails)
            .HasForeignKey(p => p.PeriodID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Employee)
            .WithMany(e => e.PayrollDetails)
            .HasForeignKey(p => p.EmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        // -- Indexes --
        builder.HasIndex(p => p.PeriodID)
            .HasDatabaseName("IX_PayrollDetails_PeriodID");

        builder.HasIndex(p => new { p.EmployeeID, p.PeriodID })
            .IsUnique()
            .HasDatabaseName("IX_PayrollDetails_Employee_Period_Unique");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_PayrollDetails_Status");
    }
}
