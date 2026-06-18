// Interfaces/Repositories/IUserRepository.cs
using NovaStaff.Models.Entities;

/*
?? METADATA - CRITICAL FOR AI IMPLEMENTATION:
TABLE: User (gi? đ?nh, check EF config)
PK: UserID (int)
Key Fields: Username(string), Email(string), EmployeeID(int?)
FK: EmployeeID ? Employee.EmployeeID (nav: Employee)
GLOBAL FILTER: IsDeleted = false (BaseEntity)
Security: PasswordHash NEVER in SELECT, IsLocked, LockoutEnd
*/

namespace NovaStaff.DataLayers.Interfaces.Repositories;

/// <summary>
/// Repository cho User (tài kho?n đăng nh?p).
///
/// User là security boundary - ch? expose minimum fields c?n thi?t.
/// NEVER return PasswordHash trong query (security).
/// 
/// Các field quan tr?ng:
///   UserID        : int, khoá chính t? tăng
///   EmployeeID    : int?, FK ? Employee (1-1 quan h?)
///   Username      : string unique, đăng nh?p chính
///   Email         : string unique, forgot password/SSO
///   PasswordHash  : string, KHÔNG bao gi? expose
///   Role          : UserRole enum (Staff/Admin/HR/Manager)
///   IsLocked      : bool, tài kho?n b? khóa
///   LockoutEnd    : DateTime?, th?i gian m? khóa t? đ?ng
///   LastLogin     : DateTime?, tracking
/// </summary>
public interface IUserRepository : IRepository<User, int>
{
    /// <summary>
    /// L?y User theo Username (login chính).
    ///
    /// Query t?i ưu index Username:
    ///   SELECT * FROM Users 
    ///   WHERE Username = @username AND IsDeleted = 0
    ///
    /// Security:
    ///   - Return null n?u không t?n t?i (tránh username enumeration attack)
    ///   - trackChanges=true cho login (c?n update LastLogin)
    ///   - include=e => e.Employee cho profile info
    ///
    /// Dùng khi:
    ///   - Authenticate username/password
    ///   - Session validation
    ///   - Role-based authorization
    /// </summary>
    Task<User?> GetByUsernameAsync(
        string username,
        bool trackChanges = false,
        Func<IQueryable<User>, IQueryable<User>>? include = null,
        CancellationToken ct = default);

    Task<User?> GetForLoginByUsernameAsync(string username, CancellationToken ct = default);

    Task<(bool Exists, bool IsLocked, DateTimeOffset? LockoutEnd)>
    GetAuthStatusByEmailAsync(
        string email,
        CancellationToken ct = default);

    // 🔒 Security
    Task IncrementFailedAttemptsAsync(int userId, CancellationToken ct = default);

    Task ResetLoginStateAsync(int userId, CancellationToken ct = default);

    Task LockUserAsync(
    int userId,
    DateTimeOffset lockoutEnd,
    CancellationToken ct = default);

    Task UpdatePasswordAsync(int userId, string passwordHash, CancellationToken ct = default);


    /// <summary>
    /// L?y User theo Email (Forgot Password, SSO).
    ///
    /// Query t?i ưu index Email:
    ///   SELECT * FROM Users 
    ///   WHERE Email = @email AND IsDeleted = 0
    ///
    /// Security:
    ///   - Return null n?u không t?n t?i (privacy)
    ///   - NEVER expose PasswordHash
    ///
    /// Dùng khi:
    ///   - Forgot password: g?i reset link qua email
    ///   - SSO: Google/AD login b?ng email
    ///   - User profile search by email
    /// </summary>
    Task<User?> GetByEmailAsync(
        string email,
        bool trackChanges = false,
        Func<IQueryable<User>, IQueryable<User>>? include = null,
        CancellationToken ct = default);

    /// <summary>
    /// Ki?m tra Username đ? t?n t?i chưa (validation).
    ///
    /// Query t?i ưu:
    ///   SELECT TOP 1 1 FROM Users 
    ///   WHERE Username = @username AND IsDeleted = 0
    ///
    /// excludeUserId: lo?i tr? User đang edit (update username)
    /// Dùng khi: Register m?i ho?c đ?i Username
    /// </summary>
    Task<bool> IsUsernameUniqueAsync(
        string username,
        int? excludeUserId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Ki?m tra Email đ? t?n t?i chưa.
    ///
    /// Query tương t? IsUsernameUniqueAsync
    /// Dùng khi: Register ho?c update email
    /// </summary>
    Task<bool> IsEmailUniqueAsync(
        string email,
        int? excludeUserId = null,
        CancellationToken ct = default);

    Task<User?> GetByEmployeeIdAsync(int employeeId, bool trackChanges = true, CancellationToken ct = default);
    Task<int?> GetEmployeeIdByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Soft-delete User khi Employee bị xóa:
    ///   1. Đánh dấu IsDeleted = true, xóa EmployeeID (orphan user)
    ///   2. Xóa tất cả ChatMember để user rời mọi kênh chat
    /// Giữ nguyên ChatMessage + MessageReaction để lịch sử chat còn đó.
    /// Gọi trước _userRepo.Delete(user) nếu muốn hard-delete, 
    /// hoặc thay thế Delete() để chỉ soft-delete.
    /// </summary>
    Task SoftDeleteChatUserAsync(int userId, DateTimeOffset deletedAt, CancellationToken ct = default);

    Task LockAsync(int userId, CancellationToken ct = default);

    Task UnlockAsync(int userId, CancellationToken ct = default);
}



