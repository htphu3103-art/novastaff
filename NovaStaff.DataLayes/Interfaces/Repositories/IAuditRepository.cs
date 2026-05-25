// Interfaces/Repositories/IAuditRepository.cs
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Interfaces.Repositories;

/// <summary>
/// Repository cho AuditLog — ch? h? tr? READ, không có Write/Update/Delete.
///
/// T?i sao không k? th?a IRepository&lt;AuditLog, long&gt;?
///   1. AuditLog là immutable — m?t khi ð? ghi không bao gi? s?a hay xóa.
///      Expose Update/Delete t?o r?i ro làm sai audit trail.
///   2. AuditLog không k? th?a BaseEntity (ðúng — không c?n audit c?a audit,
///      không c?n soft delete log).
///   3. Write cho AuditLog do AuditInterceptor.SavedChangesAsync() x? l? hoàn toàn
///      sau m?i SaveChanges — không ai ðý?c ghi th?ng vào b?ng này.
///
/// Schema AuditLog th?c t?:
///   AuditID (long)   : khoá chính t? tãng
///   TableName        : tên b?ng b? thay ð?i (MaxLength 100)
///   Action           : AuditAction enum (Insert/Update/Delete/Unknown)
///   RecordID         : ID c?a record b? thay ð?i (string, d?ng "1001")
///   OldData          : JSON snapshot trý?c khi s?a (null n?u Insert)
///   NewData          : JSON snapshot sau khi s?a (null n?u Delete)
///   ChangedBy        : UserId c?a ngý?i th?c hi?n (MaxLength 100)
///   ChangedDate      : th?i ði?m thay ð?i
///   IPAddress        : IP client (MaxLength 50)
///   UserAgent        : tr?nh duy?t/app client
/// </summary>
public interface IAuditRepository
{
    /// <summary>
    /// L?y l?ch s? thay ð?i c?a m?t b?ng c? th?.
    ///
    /// Dùng khi: admin xem "b?ng Employees có nh?ng thay ð?i g? g?n ðây".
    /// Nên k?t h?p filter th?i gian ? t?ng g?i ð? tránh load toàn b? log.
    ///
    /// Ví d?: GetByTableAsync("Employees")
    ///   ? t?t c? AuditLog có TableName = "Employees", sort ChangedDate DESC.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByTableAsync(
        string tableName,
        CancellationToken ct = default);

    /// <summary>
    /// L?y l?ch s? thay ð?i c?a m?t record c? th? trong m?t b?ng.
    ///
    /// Dùng khi: xem timeline "Employee ID=5 ð? b? s?a g?, b?i ai, lúc nào".
    ///
    /// Ví d?: GetByRecordAsync("Employees", "5")
    ///   ? [Insert 08:00 b?i admin] [Update lýõng 14:30 b?i hr_01] ...
    ///   RecordID trong AuditLog ðý?c lýu d?ng string ("5", "1001"...)
    ///   ð? tránh JOIN ph?c t?p và h? tr? c? int l?n long key.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByRecordAsync(
        string tableName,
        string recordId,
        CancellationToken ct = default);

    /// <summary>
    /// L?y toàn b? hành ð?ng c?a m?t user trên m?i b?ng.
    ///
    /// Dùng khi: ði?u tra "User ID=3 ð? làm g? trong h? th?ng hôm nay".
    /// ChangedBy trong AuditLog = ICurrentUserService.GetUserId() lúc thao tác.
    ///
    /// C?ng dùng ð? generate báo cáo ho?t ð?ng ð?nh k? cho HR/IT audit.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByUserAsync(
        string userId,
        CancellationToken ct = default);
}



