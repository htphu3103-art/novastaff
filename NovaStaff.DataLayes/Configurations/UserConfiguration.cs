using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.DataLayers.Helpers;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // -- Base Audit Fields --
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        // -- Table & Constraints --
        builder.ToTable("Users", b =>
        {
            b.HasCheckConstraint("CK_User_Username", "LENGTH(TRIM(\"Username\")) > 0");

            b.HasCheckConstraint("CK_User_Role",
                EnumConstraintHelper.BuildInConstraint<UserRole>("Role"));

            b.HasCheckConstraint("CK_User_FailedLogins",
                "\"FailedLoginAttempts\" >= 0");

            b.HasCheckConstraint("CK_User_LockoutEnd",
                "\"LockoutEnd\" IS NULL OR \"LockoutEnd\" > '2000-01-01'");
        });

        // -- Primary Key --
        builder.HasKey(u => u.UserID);
        builder.Property(u => u.UserID)
            .UseIdentityColumn();

        // -- Properties --
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("varchar(255)");

        builder.Property(u => u.Role)
            .IsRequired()
            .HasDefaultValue(UserRole.Staff)
            .HasSentinel(UserRole.Unknown);

        builder.Property(u => u.IsLocked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.FailedLoginAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.LastLogin);

        builder.Property(u => u.LastPasswordChange);

        builder.Property(u => u.LockoutEnd);

        // -- Relationships --
        builder.HasOne(u => u.Employee)
            .WithOne(e => e.User)
            .HasForeignKey<User>(u => u.EmployeeID)
            .OnDelete(DeleteBehavior.Cascade);

        // -- Indexes --
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("IX_Users_Username");

        builder.HasIndex(u => u.EmployeeID)
            .IsUnique()
            .HasFilter("\"EmployeeID\" IS NOT NULL")
            .HasDatabaseName("IX_Users_EmployeeID");

        builder.HasIndex(u => new { u.IsLocked, u.LockoutEnd })
            .HasDatabaseName("IX_Users_LockStatus");

        builder.HasIndex(u => new { u.IsLocked, u.Role })
            .HasDatabaseName("IX_Users_Status_Role");

        builder.HasIndex(u => new { u.IsLocked, u.LockoutEnd, u.FailedLoginAttempts })
            .HasDatabaseName("IX_Users_Lock_Optimize");
    }
}
