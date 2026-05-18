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
using NovaStaff.Services.Interfaces;
using System.Security.Claims;
using System.Text;
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
builder.Services.AddControllers();
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
        Description = "Nhập: Bearer {your token}"
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

// Đọc danh sách tên miền từ appsettings.json
var origins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>();

if (origins is null || origins.Length == 0)
    throw new InvalidOperationException(
        "AllowedOrigins chưa được cấu hình trong appsettings.json.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();

    // ví dụ nếu dùng nginx local
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
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.UseHierarchyId()
    );
    options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});

// ================================================================
// 3. REPOSITORIES (Đ? m? các Repo quan tr?ng)
// ================================================================
builder.Services.AddScoped(typeof(IRepository<,>), typeof(GenericRepository<,>));
builder.Services.AddSingleton<IDateTimeService, DateTimeService>();
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
// ================================================================
// 5. JWT Authentication (PHẢI Ở ĐÂY)
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
});

// ================================================================
// 6. BUILD APP PIPELINE (TH? T? C?C K? QUAN TR?NG)
// ================================================================
var app = builder.Build();

// Middleware x? l? l?i toàn c?c nên đ?t đ?u tiên
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



// ?? LƯU ?: N?u React g?i HTTP (không có S) th? comment d?ng HttpsRedirection l?i đ? test d? hơn
// app.UseHttpsRedirection(); 

// ? KÍCH HO?T CORS: aPh?i đ?t trư?c Authentication và MapControllers
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

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


