using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.DataLayers.Helpers;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class PayrollPeriodConfiguration : IEntityTypeConfiguration<PayrollPeriod>
{
    public void Configure(EntityTypeBuilder<PayrollPeriod> builder)
    {
        // -- Base Audit Fields --
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        // -- Bảng & Check Constraints --
        builder.ToTable("PayrollPeriods", b =>
        {
            b.HasCheckConstraint("CK_PayrollPeriod_Month", "\"Month\" BETWEEN 1 AND 12");
            b.HasCheckConstraint("CK_PayrollPeriod_DateRange", "\"EndDate\" >= \"StartDate\"");
            b.HasCheckConstraint(
                "CK_PayrollPeriod_Status",
                EnumConstraintHelper.BuildInConstraint<PayrollStatus>("Status")
            );
        });

        // -- Primary Key --
        builder.HasKey(p => p.PeriodID);
        builder.Property(p => p.PeriodID).UseIdentityColumn();

        // -- Month & Year --
        builder.Property(p => p.Month)
            .IsRequired();

        builder.Property(p => p.Year)
            .IsRequired();

        // -- Date Range --
        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.Property(p => p.EndDate)
            .IsRequired();

        // -- Status --
        builder.Property(p => p.Status)
            .IsRequired()
            .HasDefaultValue(PayrollStatus.Draft)
            .HasSentinel(PayrollStatus.Unknown);

        // -- Indexes --
        builder.HasIndex(p => new { p.Month, p.Year })
            .IsUnique()
            .HasDatabaseName("IX_PayrollPeriods_Month_Year");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_PayrollPeriods_Status");
    }
}
