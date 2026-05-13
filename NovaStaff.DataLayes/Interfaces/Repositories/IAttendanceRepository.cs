// Interfaces/Repositories/IAttendanceRepository.cs
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Interfaces.Repositories;

/// <summary>
/// Repository ð?c thù cho AttendanceRecord — d? li?u ch?m công.
///
/// B?ng tãng trý?ng nhanh nh?t: N nhân viên × 365 ngày/nãm.
/// M?i query PH?I có filter EmployeeID + th?i gian ð? tránh full table scan.
///
/// Các field quan tr?ng:
///   RecordID   : long, khoá chính (t? tãng — dùng long ð? không tràn s?)
///   EmployeeID : int?, FK ? Employee.EmployeeID
///   WorkDate   : DateTime, ngày làm vi?c
///   CheckIn    : DateTime?, gi? vào
///   CheckOut   : DateTime?, gi? ra (null n?u chýa check-out)
///   WorkHours  : decimal?, computed column trong DB (CheckOut - CheckIn)
///   Status     : AttendanceStatus enum
/// </summary>
public interface IAttendanceRepository : IRepository<AttendanceRecord, long>
{
    /// <summary>
    /// L?y records ch?m công c?a nhân viên trong m?t tháng.
    ///
    /// SQL ðý?c t?i ýu v?i index trên (EmployeeID, WorkDate):
    ///   SELECT * FROM AttendanceRecords
    ///   WHERE EmployeeID = @id
    ///   AND YEAR(WorkDate) = @year AND MONTH(WorkDate) = @month
    ///   AND IsDeleted = 0
    ///   ORDER BY WorkDate ASC
    ///
    /// Dùng khi:
    ///   - Nhân viên xem b?ng công tháng c?a m?nh.
    ///   - HR review b?ng công trý?c khi ch?t lýõng (k?t h?p PayrollPeriod).
    ///   - Tính lýõng: ð?m s? ngày công th?c t? trong tháng.
    /// </summary>
    Task<IEnumerable<AttendanceRecord>> GetByEmployeeAndMonthAsync(
        int employeeId,
        int year,
        int month,
        CancellationToken ct = default);

    /// <summary>
    /// L?y record ch?m công hôm nay c?a nhân viên.
    /// Tr? v? null n?u nhân viên chýa check-in hôm nay.
    ///
    /// "Hôm nay" theo WorkDate (Date only, không có time component).
    /// Dùng IDateTimeService.LocalNow.Date (gi? VN) thay v? DateTime.UtcNow.Date
    /// tránh edge case: 23:30 UTC = 06:30 sáng hôm sau VN ? sai ngày.
    ///
    /// Dùng khi:
    ///   - Nhân viên b?m Check-in: ki?m tra ð? check-in chýa, tránh duplicate RecordID.
    ///   - Nhân viên b?m Check-out: l?y record ð? update CheckOut và tính WorkHours.
    ///   - Dashboard realtime: hi?n th? tr?ng thái có m?t / v?ng m?t.
    /// </summary>
    Task<AttendanceRecord?> GetTodayAsync(int employeeId, CancellationToken ct = default);

    /// <summary>
    /// Tính t?ng s? gi? làm vi?c th?c t? c?a nhân viên trong tháng.
    ///
    /// D?a trên WorkHours (decimal?, computed column):
    ///   SUM(WorkHours) WHERE EmployeeID = @id
    ///   AND YEAR(WorkDate) = @year AND MONTH(WorkDate) = @month
    ///   AND WorkHours IS NOT NULL  -- b? qua ngày chýa check-out
    ///   AND IsDeleted = 0
    ///
    /// Tr? v? double (t?ng gi?) ð? tính lýõng:
    ///   totalHours * hourlyRate = lýõng theo gi? công th?c t?.
    ///
    /// Dùng khi: PayrollService tính lýõng cho h?p ð?ng theo gi?
    /// ho?c tính overtime (gi? làm vý?t chu?n).
    /// </summary>
    Task<double> GetTotalHoursAsync(
        int employeeId,
        int year,
        int month,
        CancellationToken ct = default);
}



