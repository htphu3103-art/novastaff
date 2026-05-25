using Microsoft.EntityFrameworkCore;
using NovaStaff.Models.Common;
using NovaStaff.Models.Entities;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NovaStaff.DataLayers;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    #region DbSets
    // --- Qu?n l? Nhân s? ---
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();

    // --- Qu?n l? Công vi?c & Ngh? phép ---
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    // --- Qu?n l? Ch?m công & Lương ---
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollDetail> PayrollDetails => Set<PayrollDetail>();

    // --- RefreshTokens ---
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    // --- H? th?ng ---
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Chat DbSets
    public DbSet<ChatChannel> ChatChannels => Set<ChatChannel>();
    public DbSet<ChatMember> ChatMembers => Set<ChatMember>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<MessageReaction> MessageReactions => Set<MessageReaction>();
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // C?u h?nh Sequence
        modelBuilder.HasSequence<int>("EmployeeCodeSequence")
            .StartsAt(1000)
            .IncrementsBy(1);

        // T? đ?ng load t?t c? class th?c thi IEntityTypeConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global Precision cho Decimal
        ConfigureDecimalPrecision(modelBuilder);

        // Global Query Filter cho Soft Delete
        //ApplySoftDeleteFilter(modelBuilder);
    }

    private static void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
    {
        var decimalProperties = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

        foreach (var property in decimalProperties)
        {
            // Ch? set n?u chưa đư?c c?u h?nh th? công trong file Configuration
            if (property.GetPrecision() == null)
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }
    }

    //private static void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
    //{
    //    var setFilterMethod = typeof(AppDbContext)
    //        .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static);

    //    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    //    {
    //        // Ch? áp d?ng cho các Entity k? th?a BaseEntity
    //        if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
    //        {
    //            setFilterMethod?.MakeGenericMethod(entityType.ClrType)
    //                .Invoke(null, new object[] { modelBuilder });
    //        }
    //    }
    //}

    

    public static class DbContextStorage
    {
        private static readonly ConditionalWeakTable<DbContext, Dictionary<string, object>> _data
            = new();

        public static Dictionary<string, object> GetOrCreate(DbContext context)
            => _data.GetOrCreateValue(context);
    }
}



