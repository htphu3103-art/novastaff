// Interfaces/Repositories/ILeaveRequestRepository.cs
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Interfaces.Repositories;

/// <summary>
/// Repository ð?c thù cho LeaveRequest — ðõn xin ngh? phép.
///
/// LeaveRequest có v?ng ð?i tr?ng thái (LeaveRequestStatus):
///   Pending ? Approved / Rejected
///
/// Các field quan tr?ng:
///   RequestID  : int, khoá chính
///   EmployeeID : int?, FK ? Employee.EmployeeID
///   FromDate   : DateTime, ngày b?t ð?u ngh?
///   ToDate     : DateTime, ngày k?t thúc ngh?
///   TotalDays  : double, T?NG S? NGÀY NGH? TH?C T? (ð? tr? T7, CN, L?, ho?c ngh? n?a ngày).
///                Tính s?n ? t?ng Service trý?c khi Insert xu?ng DB.
///   LeaveType  : enum (Annual, Sick, Unpaid...)
///   Status     : enum (Pending, Approved, Rejected)
///   ApprovedBy : int?, EmployeeID c?a ngý?i duy?t
///   ApprovedDate: DateTime?, th?i ði?m duy?t
/// </summary>
public interface ILeaveRequestRepository : IRepository<LeaveRequest, int>
{
    /// <summary>
    /// L?y toàn b? l?ch s? ðõn ngh? phép c?a m?t nhân viên (m?i tr?ng thái).
    ///
    /// Dùng khi:
    ///   - Nhân viên xem l?ch s? ngh? phép c?a m?nh (My Leave History).
    ///   - HR xem l?ch s? c?a m?t nhân viên c? th? khi x? l? khi?u n?i.
    ///
    /// Ðý?c s?p x?p theo FromDate DESC — ðõn m?i nh?t lên ð?u.
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(
        int employeeId,
        CancellationToken ct = default);

    /// <summary>
    /// L?y danh sách ðõn ðang ch? duy?t (Status = Pending).
    ///
    /// T?i ýu hi?u nãng: Truy?n departmentId s? ð?y vi?c filter xu?ng t?n Database (SQL WHERE),
    /// thay v? kéo hàng ngàn ðõn lên RAM r?i dùng LINQ in-memory ð? l?c.
    ///
    /// Dùng khi: 
    ///   - HR xem toàn b? ðõn Pending công ty (ð? departmentId = null).
    ///   - Manager xem ðõn Pending c?a riêng ph?ng ban m?nh (truy?n departmentId).
    /// Nên sort theo CreatedDate ASC — ðõn n?p s?m nh?t ýu tiên x? l? trý?c.
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetPendingAsync(
        int? departmentId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Tính t?ng s? ngày ngh? ð? ðý?c duy?t c?a nhân viên trong m?t nãm c? th?.
    ///
    /// B? qua cách tính (ToDate - FromDate) v? dính b?y ngày L?/Cu?i tu?n.
    /// Thay vào ðó, query s? g?i: SUM(TotalDays) 
    /// (TotalDays là c?t ð? ðý?c Service tính chu?n xác lúc n?p ðõn).
    ///
    /// Ch? ð?m Status = Approved:
    ///   Pending  ? chýa ch?c ðý?c duy?t, không tính.
    ///   Rejected ? không b? tr? phép.
    ///
    /// Dùng khi: Ki?m tra s? dý phép trý?c khi duy?t ðõn m?i.
    ///   Ví d?: Qu? phép 12 ngày, hàm này tr? v? 10.5 ? c?n dý 1.5 ngày.
    /// </summary>
    Task<double> CountApprovedDaysAsync(
        int employeeId,
        int year,
        CancellationToken ct = default);
}



