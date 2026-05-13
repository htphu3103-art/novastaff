// DataLayers/Repositories/PayrollRepository.cs
using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.DataLayers.Repositories;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;

namespace NovaStaff.DataLayers.Repositories;

/// <summary>
/// Triển khai IPayrollRepository — Financial Grade, không được sai số.
///
/// SNAPSHOT PRINCIPLE: BaseSalarySnapshot đọc tĩnh, KHÔNG join live Employee.BaseSalary.
/// IMMUTABLE RULE: PayrollPeriod ở trạng thái Paid KHÔNG được Update/Delete.
/// </summary>
public class PayrollRepository : GenericRepository<PayrollPeriod, int>, IPayrollRepository
{
    // DbSet tắt cho PayrollDetails — truy cập trực tiếp thay vì qua navigation
    private readonly DbSet<PayrollDetail> _details;
    private readonly DbSet<Employee> _employees;

    public PayrollRepository(AppDbContext context) : base(context)
    {
        _details = context.Set<PayrollDetail>();
        _employees = context.Set<Employee>();
    }

    // =========================================================
    // 1. GetActiveAsync
    //    Trả về kỳ lương chưa Paid (Draft / Calculated / Approved).
    //    Business rule: tối đa 1 kỳ đang mở cùng lúc.
    // =========================================================

    public async Task<PayrollPeriod?> GetActiveAsync(CancellationToken ct = default)
    {
        // Lấy kỳ mới nhất chưa Paid — dùng FirstOrDefaultAsync thay vì SingleOrDefaultAsync
        // để tránh exception nếu data bị inconsistent (defensive coding).
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.Status != PayrollStatus.Paid)
            .OrderByDescending(p => p.PeriodID)
            .FirstOrDefaultAsync(ct);
    }

    // =========================================================
    // 2. GetDetailsByPeriodAsync
    //    Eager load Employee → Department để HR review / export Excel.
    //    Filter optional theo departmentId.
    // =========================================================

    public async Task<IEnumerable<PayrollDetail>> GetDetailsByPeriodAsync(
        int periodId,
        int? departmentId = null,
        CancellationToken ct = default)
    {
        var query = _details
            .AsNoTracking()
            .Include(pd => pd.Employee)
                .ThenInclude(e => e!.Department)
            .Where(pd => pd.PeriodID == periodId);

        // Filter phòng ban — push xuống DB, không filter in-memory
        if (departmentId.HasValue)
            query = query.Where(pd =>
                pd.Employee != null &&
                pd.Employee.DepartmentID == departmentId.Value);

        // Sort ổn định: Department → FullName để xuất Excel dễ đọc
        return await query
            .OrderBy(pd => pd.Employee!.Department!.DepartmentName)
            .ThenBy(pd => pd.Employee!.FullName)
            .ToListAsync(ct);
    }

    // =========================================================
    // 3. GetDetailByEmployeeAsync
    //    Composite index (PeriodID, EmployeeID) — O(log n).
    //    Dùng để xem payslip cá nhân hoặc check duplicate.
    // =========================================================

    public async Task<PayrollDetail?> GetDetailByEmployeeAsync(
    int periodId,
    int employeeId,
    bool trackChanges = false,
    CancellationToken ct = default)
    {
        IQueryable<PayrollDetail> query = trackChanges
            ? _details
            : _details.AsNoTracking();

        return await query
            .Include(pd => pd.Employee)
                .ThenInclude(e => e!.Department)
            .Include(pd => pd.Period)
            .FirstOrDefaultAsync(
                pd => pd.PeriodID == periodId &&
                      pd.EmployeeID == employeeId,
                ct);
    }

    // =========================================================
    // 4. GetTotalNetSalaryAsync
    //    Pure scalar aggregate — KHÔNG load entity vào memory.
    //    SumAsync trả về 0 nếu không có row (EF Core behavior).
    // =========================================================

    public async Task<decimal> GetTotalNetSalaryAsync(
        int periodId,
        CancellationToken ct = default)
    {
        return await _details
            .Where(pd => pd.PeriodID == periodId)
            .SumAsync(pd => pd.NetSalary, ct);
    }

    // =========================================================
    // 5. GetMissingDetailsAsync
    //    Anti-gap: tìm Employee CHƯA có PayrollDetail trong kỳ.
    //    LEFT JOIN + IS NULL pattern — hiệu quả hơn NOT IN / NOT EXISTS
    //    khi tập employee lớn.
    // =========================================================

    public async Task<IEnumerable<Employee>> GetMissingDetailsAsync(
        int periodId,
        int? departmentId = null,
        CancellationToken ct = default)
    {
        // Lấy tập EmployeeID đã có PayrollDetail trong kỳ này
        // SubQuery nhỏ — EF dịch thành EXISTS hoặc IN tùy provider
        var existingIds = _details
            .Where(pd => pd.PeriodID == periodId)
            .Select(pd => pd.EmployeeID);

        var query = _employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => !existingIds.Contains(e.EmployeeID));

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentID == departmentId.Value);

        return await query
            .OrderBy(e => e.DepartmentID)
            .ThenBy(e => e.FullName)
            .ToListAsync(ct);
    }

    // =========================================================
    // 6. CountByStatusAsync
    //    GROUP BY Status trong DB — KHÔNG load toàn bộ PayrollDetail.
    //    Trả về Dictionary đầy đủ mọi PayrollStatus (kể cả count = 0).
    // =========================================================

    public async Task<Dictionary<PayrollStatus, int>> CountByStatusAsync(
        int periodId,
        CancellationToken ct = default)
    {
        // Aggregate trong DB
        var rawCounts = await _details
            .Where(pd => pd.PeriodID == periodId)
            .GroupBy(pd => pd.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        // Khởi tạo dictionary với tất cả enum values = 0
        // → caller không cần check ContainsKey
        var result = Enum.GetValues<PayrollStatus>()
            .ToDictionary(s => s, _ => 0);

        foreach (var item in rawCounts)
            result[item.Status] = item.Count;

        return result;
    }
}