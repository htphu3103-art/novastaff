// DataLayers/Repositories/RefreshTokenRepository.cs
using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Entities;

namespace NovaStaff.DataLayers.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeService _clock;

    public RefreshTokenRepository(
        AppDbContext context,
        IDateTimeService clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task AddAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
    }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
    }

    public async Task<RefreshToken?> GetActiveAsync(
    string tokenHash)
    {
        var now = _clock.UtcNow;

        return await _context.RefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == tokenHash &&
                rt.RevokedAt == null &&
                rt.ExpiresAt > now);
    }

    public async Task RevokeAsync(string tokenHash, string? replacedBy = null)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (refreshToken is null) return;

        refreshToken.RevokedAt = _clock.UtcNow;
        refreshToken.ReplacedByTokenHash = replacedBy;
    }

    public async Task RevokeAllByUserAsync(int userId, CancellationToken ct = default)
    {
        await _context.RefreshTokens
            .Where(rt => rt.UserID == userId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(rt => rt.RevokedAt, _clock.UtcNow),
                ct);
    }
}
