using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaStaff.Infrastructure.Cache;
using NovaStaff.Infrastructure.Email;
using NovaStaff.Shared.Cache;
using NovaStaff.Shared.Email;
using StackExchange.Redis;
using NovaStaff.Infrastructure.Activation; // ← thêm using
using NovaStaff.Shared.Activation;
namespace NovaStaff.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Email
        services.Configure<EmailSettings>(config.GetSection("Email"));
        var isDevelopment = config["ASPNETCORE_ENVIRONMENT"] == "Development";
        if (isDevelopment)
            services.AddScoped<IEmailService, SmtpEmailService>();
        else
            services.AddScoped<IEmailService, ResendEmailService>();

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(
                config.GetConnectionString("Redis")!
            );
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddScoped<ICacheService, RedisCacheService>(); // ← thiếu dòng này
                                                                // Activation Token          ← thêm
        services.AddScoped<IActivationTokenService, ActivationTokenService>();
        return services; // ← thiếu dòng này
    }
}