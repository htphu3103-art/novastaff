using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.Models.Common;

namespace NovaStaff.DataLayers.Configurations;

public static class BaseEntityConfiguration
{
    public static void ConfigureBaseEntity<T>(EntityTypeBuilder<T> builder)
        where T : BaseEntity
    {
        // Created
        builder.Property(e => e.CreatedBy)
            .HasColumnType("int");

        builder.Property(e => e.CreatedByName)
            .HasMaxLength(100);

        builder.Property(e => e.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Modified
        builder.Property(e => e.ModifiedBy)
            .HasColumnType("int");

        builder.Property(e => e.ModifiedByName)
            .HasMaxLength(100);

        builder.Property(e => e.ModifiedDate);
    }
}
