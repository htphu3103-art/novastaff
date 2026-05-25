using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Interceptors;
using NovaStaff.DataLayers.Interfaces;

namespace NovaStaff.DataLayers;

public class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // =====================================================
        // 1. Load configuration
        // =====================================================
        var basePath = AppContext.BaseDirectory;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // =====================================================
        // 2. Design-time services
        // =====================================================
        var currentUser = new DesignTimeCurrentUserService();
        var auditInterceptor = new AuditInterceptor(currentUser);

        // =====================================================
        // 3. Configure DbContext
        // =====================================================
        optionsBuilder
    .UseNpgsql(connectionString)
    .AddInterceptors(auditInterceptor);
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
#endif

        return new AppDbContext(optionsBuilder.Options);
    }
}
/// <summary>
/// Fake user context cho EF Core CLI (migration / update database)
/// </summary>
internal class DesignTimeCurrentUserService : ICurrentUserService
{
    public int? GetUserId() => 0;

    public string? GetDisplayName() => "Migration_Runner";

    public string? GetIpAddress() => "127.0.0.1";

    public string? GetUserAgent() => "EF_Core_CLI";

    public bool IsAuthenticated() => false;

    public string? GetRole() => "System";

    public int? GetEmployeeId()
    {
        throw new NotImplementedException();
    }
}



