// Interfaces/Repositories/IPayrollRepository.cs
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;

/*
?? METADATA - CRITICAL FOR AI IMPLEMENTATION:
TABLES: 
  - PayrollPeriods (PK: PeriodID int)
  - PayrollDetails (PK: DetailID long, UK: PeriodID+EmployeeID)
Key Fields:
  PayrollPeriod: Month(int), Year(int), Status(PayrollStatus)
  PayrollDetail: PeriodID(int), EmployeeID(int?), BaseSalarySnapshot(decimal), 
                 NetSalary(decimal), BonusAndAllowancesJson(string), DeductionsJson(string)
Relationships:
  PayrollDetail.PeriodID ? PayrollPeriod.PeriodID (1:N)
  PayrollDetail.EmployeeID ? Employee.EmployeeID (N:1)
GLOBAL FILTER: IsDeleted = false (BaseEntity)
SNAPSHOT RULE: Static values at calculation time, NO live Employee links
*/

namespace NovaStaff.DataLayers.Interfaces.Repositories;

/// <summary>
/// Repository qu?n l? nghi?p v? Lýõng — K? lýõng (Period) + Chi ti?t (Detail).
///
/// FINANCIAL GRADE - Ð? chính xác 100%, không th? sai s?.
/// 
/// SNAPSHOT PRINCIPLE (CRITICAL):
///   - BaseSalarySnapshot lýu giá tr? t?i th?i ði?m tính (KHÔNG link Employee.BaseSalary live)
///   - JSON linh ho?t: BonusAndAllowancesJson/DeductionsJson ? Schema stable
///   - Immutable sau khi Closed ? KHÔNG cho phép Update
/// 
/// Quy tr?nh:
///   Draft ? Processing (ch?t công) ? Calculated ? HR Review ? Closed (read-only)
/// </summary>
public interface IPayrollRepository : IRepository<PayrollPeriod, int>
{
    /// <summary>
    /// L?y k? lýõng ðang Active/Processing (Status != Closed).
    ///
    /// Business Rule QUAN TR?NG:
    ///   - Ch? t?n t?i MAX 1 k? không Closed
    ///   - Query: WHERE Status IN (Draft, Processing, Calculated) ORDER BY PeriodID DESC
    ///
    /// Dùng khi:
    ///   - Ch?n t?o k? ch?ng chéo
    ///   - Push AttendanceRecords vào k? hi?n t?i
    ///   - Check "có k? ðang ch?y không" trý?c batch job
    ///
    /// Return null n?u t?t c? k? ð?u Closed
    /// </summary>
    Task<PayrollPeriod?> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// L?y chi ti?t lýõng c?a m?t k? (t?i ýu HR review).
    ///
    /// Index query PayrollDetails.PeriodID + EmployeeID:
    ///   INNER JOIN PayrollDetails pd ON pd.PeriodID = @periodId
    ///   LEFT JOIN Employee e ON e.EmployeeID = pd.EmployeeID
    ///   LEFT JOIN Department d ON d.DepartmentID = e.DepartmentID
    ///   WHERE (@departmentId IS NULL OR d.DepartmentID = @departmentId)
    ///
    /// Eager loading: .Include(pd => pd.Employee).ThenInclude(e => e.Department)
    ///
    /// Dùng khi:
    ///   - HR duy?t b?ng lýõng theo ph?ng ban
    ///   - Export Excel payslips batch
    ///   - Dashboard: lýõng k? này theo department
    /// </summary>
    Task<IEnumerable<PayrollDetail>> GetDetailsByPeriodAsync(
        int periodId,
        int? departmentId = null,
        CancellationToken ct = default);

    /// <summary>
    /// L?y phi?u lýõng c?a nhân viên trong k? c? th?.
    ///
    /// Unique constraint: (PeriodID, EmployeeID)
    /// Query t?i ýu composite index:
    ///   WHERE PeriodID = @period AND EmployeeID = @employee
    ///
    /// Dùng khi:
    ///   - Employee xem payslip cá nhân
    ///   - Check duplicate trý?c Insert PayrollDetail
    ///   - History: lýõng các k? trý?c c?a nhân viên
    /// </summary>
    Task<PayrollDetail?> GetDetailByEmployeeAsync(
    int periodId,
    int employeeId,
    bool trackChanges = false,
    CancellationToken ct = default);

    /// <summary>
    /// Tính t?ng ngân sách NetSalary c?a k? (aggregate direct).
    ///
    /// NO entity loading - pure scalar:
    ///   SELECT SUM(NetSalary) FROM PayrollDetails WHERE PeriodID = @periodId
    ///
    /// Dùng khi:
    ///   - CEO Dashboard: "T?ng lýõng tháng này"
    ///   - Budget planning: chi phí nhân s? k? t?i
    ///   - KPI: lýõng chi so v?i ngân sách
    /// </summary>
    Task<decimal> GetTotalNetSalaryAsync(int periodId, CancellationToken ct = default);

    /// <summary>
    /// L?y danh sách nhân viên chýa có PayrollDetail trong k?.
    ///
    /// Anti-gap query:
    ///   Employees e LEFT JOIN PayrollDetails pd ON pd.EmployeeID = e.EmployeeID 
    ///   WHERE pd.PeriodID = @periodId AND pd.EmployeeID IS NULL
    ///
    /// Dùng khi: Batch job "fill missing payslips"
    /// </summary>
    Task<IEnumerable<Employee>> GetMissingDetailsAsync(
        int periodId,
        int? departmentId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Ð?m s? PayrollDetail theo Status trong k?.
    ///
    /// Dashboard aggregate:
    ///   COUNT(*) GROUP BY Status WHERE PeriodID = @periodId
    ///
    /// Dùng khi: HR tracking "X payslips pending approval"
    /// </summary>
    Task<Dictionary<PayrollStatus, int>> CountByStatusAsync(
        int periodId,
        CancellationToken ct = default);
}



