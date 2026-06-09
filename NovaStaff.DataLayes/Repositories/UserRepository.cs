using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NovaStaff.DataLayers.Repositories;

public class UserRepository : GenericRepository<User, int>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetForLoginByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(u => u.Employee)
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Employee != null && u.Employee.Email == email, ct);
    }

    public async Task<(bool Exists, bool IsLocked, DateTime? LockoutEnd)> GetAuthStatusByEmailAsync(string email, CancellationToken ct = default)
    {
        var status = await _dbSet
            .AsNoTracking()
            .Where(u => u.Employee != null && u.Employee.Email == email)
            .Select(u => new { u.IsLocked, u.LockoutEnd })
            .FirstOrDefaultAsync(ct);

        if (status == null)
            return (false, false, null);

        return (true, status.IsLocked, status.LockoutEnd);
    }

    public async Task IncrementFailedAttemptsAsync(int userId, CancellationToken ct = default)
    {
        await _dbSet
            .Where(u => u.UserID == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.FailedLoginAttempts, u => u.FailedLoginAttempts + 1), ct);
    }

    public async Task ResetLoginStateAsync(int userId, CancellationToken ct = default)
    {
        await _dbSet
            .Where(u => u.UserID == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.FailedLoginAttempts, 0)
                .SetProperty(u => u.IsLocked, false)
                .SetProperty(u => u.LockoutEnd, (DateTime?)null), ct);
    }

    public async Task LockUserAsync(int userId, DateTime lockoutEnd, CancellationToken ct = default)
    {
        await _dbSet
            .Where(u => u.UserID == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.IsLocked, true)
                .SetProperty(u => u.LockoutEnd, lockoutEnd), ct);
    }

    public async Task UpdatePasswordAsync(int userId, string passwordHash, CancellationToken ct = default)
    {
        await _dbSet
            .Where(u => u.UserID == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.PasswordHash, passwordHash)
                .SetProperty(u => u.LastPasswordChange, DateTime.UtcNow), ct);
    }

    public async Task<User?> GetByUsernameAsync(string username, bool trackChanges = false, Func<IQueryable<User>, IQueryable<User>>? include = null, CancellationToken ct = default)
    {
        var query = BuildQuery(trackChanges, include);
        return await query.FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, bool trackChanges = false, Func<IQueryable<User>, IQueryable<User>>? include = null, CancellationToken ct = default)
    {
        var query = BuildQuery(trackChanges, include);
        return await query.FirstOrDefaultAsync(u => u.Employee != null && u.Employee.Email == email, ct);
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null, CancellationToken ct = default)
    {
        return !await _dbSet.AnyAsync(u =>
            u.Username == username &&
            (!excludeUserId.HasValue || u.UserID != excludeUserId.Value), ct);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null, CancellationToken ct = default)
    {
        return !await _dbSet.AnyAsync(u =>
            u.Employee != null &&
            u.Employee.Email == email &&
            (!excludeUserId.HasValue || u.UserID != excludeUserId.Value), ct);
    }

    public Task<User?> GetByEmployeeIdAsync(int employeeId, bool trackChanges = true, CancellationToken ct = default)
    {
        var query = trackChanges ? _dbSet : _dbSet.AsNoTracking();
        return query.FirstOrDefaultAsync(x => x.EmployeeID == employeeId, ct);
    }
    public async Task<int?> GetEmployeeIdByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(u => u.UserID == userId)
            .Select(u => u.EmployeeID)
            .FirstOrDefaultAsync(ct);
    }

    public async Task SoftDeleteChatUserAsync(int userId, CancellationToken ct = default)
    {
        // 1. Soft-delete user: đánh dấu IsDeleted, xóa EmployeeID (orphan),
        //    thu hồi quyền truy cập bằng cách deactivate
        await _dbSet
            .Where(u => u.UserID == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.IsDeleted, true)
                .SetProperty(u => u.DeletedAt, DateTime.UtcNow)
                .SetProperty(u => u.IsActive, false)
                .SetProperty(u => u.EmployeeID, (int?)null),
            ct);

        // 2. Xóa tất cả ChatMember để user rời mọi kênh chat
        //    (ChatMessage + MessageReaction giữ nguyên để lịch sử còn đó)
        await _context.Set<NovaStaff.Models.Entities.ChatMember>()
            .Where(cm => cm.UserID == userId)
            .ExecuteDeleteAsync(ct);
    }
}