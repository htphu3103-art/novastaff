using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaStaff.BusinessLayers.Interfaces.Redis;
using NovaStaff.BusinessLayers.Services;
using NovaStaff.Infrastructure.Activation; 
using NovaStaff.Infrastructure.Cache;
using NovaStaff.Infrastructure.Email;
using NovaStaff.Infrastructure.Token;
using NovaStaff.Shared.Activation;
using NovaStaff.Shared.Cache;
using NovaStaff.Shared.Email;
using StackExchange.Redis;
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

        services.AddScoped<ICacheService, RedisCacheService>(); 
                                                                    
        services.AddScoped<IActivationTokenService, ActivationTokenService>();
        services.AddScoped<ITokenBlacklistService, RedisTokenBlacklistService>();
        return services; 
    }
}