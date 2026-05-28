using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.BusinessLayers.Services;
using NovaStaff.DataLayers;
using NovaStaff.DataLayers.Interceptors;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.DataLayers.Repositories;
using NovaStaff.Models.Common;
using NovaStaff.Services;
using NovaStaff.Hubs;
using NovaStaff.Services.Interfaces;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using NovaStaff.Web.Middlewares;
using StackExchange.Redis;
using NovaStaff.Infrastructure;
var builder = WebApplication.CreateBuilder(args);

// ================================================================
// 1. INFRASTRUCTURE & API SERVICES
// ================================================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditInterceptor>();

// Program.cs
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nháº­p: Bearer {your token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Äá»c danh sĂ¡ch tĂªn miá»n tá»« appsettings.json
var origins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>();

if (origins is null || origins.Length == 0)
    throw new InvalidOperationException(
        "AllowedOrigins chÆ°a Ä‘Æ°á»£c cáº¥u hĂ¬nh trong appsettings.json.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin))
                    return false;

                if (origins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                    return true;

                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    return false;

                return uri.Host.EndsWith(".trycloudflare.com", StringComparison.OrdinalIgnoreCase);
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();

    // vĂ­ dá»¥ náº¿u dĂ¹ng nginx local
    // options.KnownProxies.Add(
    //     IPAddress.Parse("127.0.0.1"));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("loginPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetClientPartitionKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    options.AddPolicy("refreshPolicy", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: GetRefreshPartitionKey(httpContext),
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});


// ================================================================
// 2. DATABASE
// ================================================================
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 6,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null
            );
        }
    );

    options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});

// ================================================================
// 3. REPOSITORIES (Ä? m? cĂ¡c Repo quan tr?ng)
// ================================================================
builder.Services.AddScoped(typeof(IRepository<,>), typeof(GenericRepository<,>));
builder.Services.AddSingleton<IDateTimeService, DateTimeService>();
builder.Services.AddSingleton<PresenceTracker>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IWorkTaskRepository, WorkTaskRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
// ================================================================
// 4. BUSINESS SERVICES
// ================================================================
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWorkTaskService, WorkTaskService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<ILeaveCalculator, LeaveCalculator>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IChatService, ChatService>();
// ================================================================
// 5. JWT Authentication (PHáº¢I á» ÄĂ‚Y)
// ================================================================
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt")
);

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwt = jwtSection.Get<JwtSettings>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,

        ValidIssuer = jwt!.Issuer,
        ValidAudience = jwt.Audience,

        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt.Key)
        ),

        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // Nếu request là từ SignalR Hub (có chữ chathub)
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
            {
                // Trích xuất token từ query string để chứng thực
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// ================================================================
// 6. BUILD APP PIPELINE (TH? T? C?C K? QUAN TR?NG)
// ================================================================
var app = builder.Build();

app.UseForwardedHeaders();

// Middleware x? l? l?i toĂ n c?c nĂªn Ä‘?t Ä‘?u tiĂªn
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



// ?? LÆ¯U ?: N?u React g?i HTTP (khĂ´ng cĂ³ S) th? comment d?ng HttpsRedirection l?i Ä‘? test d? hÆ¡n
// app.UseHttpsRedirection(); 

// ? KĂCH HO?T CORS: aPh?i Ä‘?t trÆ°?c Authentication vĂ  MapControllers
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapHub<ChatHub>("/chathub");
app.MapControllers();

// ================================================================
// 7. AUTO MIGRATION & DB CONNECTIVITY RETRY
// ================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<AppDbContext>();

    const int maxRetries = 6;
    const int delaySeconds = 5;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation(
                "Applying migrations (Attempt {Attempt}/{MaxRetries})...",
                attempt, maxRetries);

            await context.Database.MigrateAsync();

            logger.LogInformation("Database migration successful.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Migration failed (Attempt {Attempt}/{MaxRetries})",
                attempt, maxRetries);

            if (attempt == maxRetries)
            {
                logger.LogError(ex, "Migration failed permanently.");
                // Graceful exit
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
    }
}

app.Run();

static string GetRefreshPartitionKey(HttpContext httpContext)
{
    return GetClientPartitionKey(httpContext);
}

static string GetClientPartitionKey(HttpContext httpContext)
{
    return httpContext.Connection
        .RemoteIpAddress?
        .ToString()
        ?? "unknown";
}


