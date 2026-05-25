using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.DataLayers.Helpers;
using NovaStaff.DataLayers.Configurations;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        var statusConstraint = EnumConstraintHelper.BuildInConstraint<EmployeeStatus>(nameof(Employee.Status));
        var genderConstraint = EnumConstraintHelper.BuildInConstraint<GenderType>(nameof(Employee.Gender));

        builder.ToTable("Employees", b =>
        {
            b.HasCheckConstraint("CK_Employee_Status", statusConstraint);
            b.HasCheckConstraint("CK_Employee_Gender", genderConstraint);
            b.HasCheckConstraint("CK_Employee_BaseSalary", "\"BaseSalary\" >= 0");
        });

        builder.HasKey(e => e.EmployeeID);
        builder.Property(e => e.EmployeeID).UseIdentityColumn();

        // --- CẤU HÌNH CỘT ---
        builder.Property(e => e.EmployeeCode)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValueSql("nextval('\"EmployeeCodeSequence\"')");

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.BaseSalary)
            .HasPrecision(18, 4)
            .HasComment("Lương cơ bản hàng tháng");

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.Address)
            .HasMaxLength(255);

        // --- CẤU HÌNH QUẢN LÝ TRỰC TIẾP (Self-Referencing) ---
        builder.Property(e => e.SupervisorID)
            .HasColumnType("int");

        builder.HasOne(e => e.Supervisor)
            .WithMany(s => s.Subordinates)
            .HasForeignKey(e => e.SupervisorID)
            .OnDelete(DeleteBehavior.Restrict);

        // --- RELATIONSHIPS ---
        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentID)
            .OnDelete(DeleteBehavior.Restrict);

        // --- INDEXES ---
        builder.HasIndex(e => e.EmployeeCode)
            .IsUnique();

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.HasIndex(e => e.SupervisorID)
            .HasDatabaseName("IX_Employees_SupervisorID");

        builder.HasIndex(e => new { e.FullName, e.Status })
            .HasDatabaseName("IX_Employees_Search_NameStatus");
    }
}
