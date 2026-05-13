// Services/AuthService.cs
using Microsoft.Extensions.Options;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Auth;
using NovaStaff.Models.Entities;
using NovaStaff.Services.Interfaces;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _clock;
    private readonly JwtSettings _jwt;

    public AuthService(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        ITokenService tokenService,
        IUnitOfWork uow,
        IDateTimeService clock,
        IOptions<JwtSettings> jwtOptions)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _tokenService = tokenService;
        _uow = uow;
        _clock = clock;
        _jwt = jwtOptions.Value;
    }

    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var user = await _userRepo.GetForLoginByEmailAsync(email);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password");

        var now = _clock.UtcNow;

        if (user.IsLocked && user.LockoutEnd > now)
            throw new UnauthorizedAccessException("Account is locked");

        if (user.IsLocked && user.LockoutEnd <= now)
            await _userRepo.ResetLoginStateAsync(user.UserID);

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            await HandleFailedLogin(user, now);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        await _userRepo.ResetLoginStateAsync(user.UserID);
        user.LastLogin = now;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        await _refreshTokenRepo.AddAsync(new RefreshToken
        {
            UserID = user.UserID,
            Token = refreshTokenValue,
            CreatedAt = now,
            ExpiresAt = now.AddDays(_jwt.RefreshTokenDays)
        });

        await _uow.SaveChangesAsync();

        return new LoginResponse(accessToken, refreshTokenValue);
    }

    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        var now = _clock.UtcNow;

        var storedToken = await _refreshTokenRepo.GetActiveAsync(refreshToken);

        if (storedToken is null || !storedToken.IsActive(now))
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = await _userRepo.GetByIdAsync(storedToken.UserID);

        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        var newRefreshToken = _tokenService.GenerateRefreshToken();

        await _refreshTokenRepo.RevokeAsync(refreshToken, replacedBy: newRefreshToken);

        await _refreshTokenRepo.AddAsync(new RefreshToken
        {
            UserID = user.UserID,
            Token = newRefreshToken,
            CreatedAt = now,
            ExpiresAt = now.AddDays(_jwt.RefreshTokenDays)
        });

        var newAccessToken = _tokenService.GenerateAccessToken(user);

        await _uow.SaveChangesAsync();

        return newAccessToken;
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var storedToken = await _refreshTokenRepo.GetActiveAsync(refreshToken);

        if (storedToken is null)
            throw new UnauthorizedAccessException("Token not found");

        await _refreshTokenRepo.RevokeAsync(refreshToken);
        await _uow.SaveChangesAsync();
    }

    private async Task HandleFailedLogin(User user, DateTime now)
    {
        const int MAX_ATTEMPTS = 5;
        const int LOCK_MINUTES = 15;

        var attempts = user.FailedLoginAttempts + 1;

        if (attempts >= MAX_ATTEMPTS)
        {
            await _userRepo.LockUserAsync(
                user.UserID,
                now.AddMinutes(LOCK_MINUTES)
            );
        }
        else
        {
            await _userRepo.IncrementFailedAttemptsAsync(user.UserID);
        }

        await _uow.SaveChangesAsync();
    }
}