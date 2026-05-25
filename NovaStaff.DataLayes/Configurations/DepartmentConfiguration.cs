using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        // -- Base Audit Fields --
        BaseEntityConfiguration.ConfigureBaseEntity(builder);

        // -- Bảng & Constraints --
        builder.ToTable("Departments", t =>
        {
            t.HasCheckConstraint("CK_Department_Name", "LENGTH(TRIM(\"DepartmentName\")) > 0");
            t.HasCheckConstraint("CK_Department_Code", "\"Code\" IS NULL OR LENGTH(TRIM(\"Code\")) > 0");
        });

        // -- Primary Key --
        builder.HasKey(d => d.DepartmentID);
        builder.Property(d => d.DepartmentID)
               .UseIdentityColumn();

        // -- Properties --
        builder.Property(d => d.DepartmentName)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Tên phòng ban/bộ phận");

        builder.Property(d => d.Code)
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasComment("Mã định danh phòng ban");

        builder.Property(d => d.OrgPath)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Đường dẫn phân cấp phòng ban (Materialized Path)");

        builder.Property(d => d.OrgLevel)
            .HasColumnType("smallint")
            .IsRequired()
            .HasComment("Cấp bậc phòng ban trong cây");

        // Để đổi tên thành ManagerEmployeeID để đồng bộ với bảng Employee
        builder.Property(d => d.ManagerEmployeeID) 
            .HasColumnType("int")
            .HasComment("ID nhân viên đang giữ chức vụ trưởng phòng");

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        // -- Relationships --

        // 1. Một phòng ban có nhiều nhân viên
        builder.HasMany(d => d.Employees)
            .WithOne(e => e.Department)
            .HasForeignKey(e => e.DepartmentID)
            .OnDelete(DeleteBehavior.Restrict);

        // 2. Một phòng ban có MỘT trưởng phòng (trở về bảng Employee)
        builder.HasOne(d => d.Manager)
            .WithMany()
            .HasForeignKey(d => d.ManagerEmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        // -- Indexes --
        builder.HasIndex(d => d.OrgPath)
            .IsUnique()
            .HasDatabaseName("IX_Departments_OrgPath");

        builder.HasIndex(d => d.Code)
            .IsUnique()
            .HasDatabaseName("IX_Departments_Code");

        // Index hỗ trợ tìm kiếm theo Trưởng phòng nhanh hơn
        builder.HasIndex(d => d.ManagerEmployeeID)
            .HasDatabaseName("IX_Departments_ManagerEmployeeID");
    }
}
