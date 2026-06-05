using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        // Nếu đã có user rồi thì không seed nữa
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Database đã có dữ liệu, bỏ qua seed.");
            return;
        }

        logger.LogInformation("Bắt đầu seed dữ liệu...");

        // 1. Tạo Employee admin
        var adminEmployee = new Employee
        {
            EmployeeCode = "EMP001",
            FullName = "Administrator",
            Gender = GenderType.Other,
            Email = "admin@novastaff.com",
            BaseSalary = 0,
            Status = EmployeeStatus.Active,
            JoinDate = DateTime.UtcNow,
            Position = "System Administrator"
        };

        context.Employees.Add(adminEmployee);
        await context.SaveChangesAsync();

        // 2. Tạo User admin
        var adminUser = new User
        {
            EmployeeID = adminEmployee.EmployeeID,
            Username = "admin@novastaff.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
            Role = UserRole.Admin,
            IsActive = true,
            LastPasswordChange = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        logger.LogInformation("Seed dữ liệu hoàn tất. Tài khoản admin: admin@novastaff.com / Admin@123456");
    }
}
