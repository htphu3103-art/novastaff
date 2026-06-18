using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
    AppDbContext context,
    ILogger logger)
    {
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation(
                "Database đã có dữ liệu, bỏ qua seed.");
            return;
        }

        logger.LogInformation(
            "Bắt đầu seed dữ liệu...");

        var adminUser = new User
        {
            Username = "admin",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    "Admin@123456"),

            Role = UserRole.Admin,

            IsActive = true,

            LastPasswordChange =
                DateTimeOffset.UtcNow
        };

        context.Users.Add(adminUser);

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Seed admin thành công.");
    }
}
