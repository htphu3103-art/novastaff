// Services/AuthService.cs
using Microsoft.Extensions.Options;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Helpers;
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Auth;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Requests;
using NovaStaff.Services.Interfaces;
using NovaStaff.Shared.Activation;
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _clock;
    private readonly JwtSettings _jwt;
    private readonly IActivationTokenService _activationTokenService;

    public AuthService(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        ITokenService tokenService,
        IUnitOfWork uow,
        IDateTimeService clock,
        IOptions<JwtSettings> jwtOptions,
        IActivationTokenService activationTokenService)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _tokenService = tokenService;
        _uow = uow;
        _clock = clock;
        _jwt = jwtOptions.Value;
        _activationTokenService = activationTokenService;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var user = await _userRepo.GetForLoginByUsernameAsync(username);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản chưa được kích hoạt. Vui lòng kiểm tra email.");

        var now = _clock.UtcNow.UtcDateTime; // 🔥 FIX 1

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

        user.LastLogin = now; // ✔ OK

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = TokenHasher.Hash(refreshTokenValue);

        await _refreshTokenRepo.AddAsync(new RefreshToken
        {
            UserID = user.UserID,
            TokenHash = refreshTokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(_jwt.RefreshTokenDays)
        });

        await _uow.SaveChangesAsync();

        return new LoginResponse(accessToken, refreshTokenValue);
    }

    public async Task<RefreshResponse> RefreshTokenAsync(string refreshToken)
    {
        var now = _clock.UtcNow;
        var refreshTokenHash = TokenHasher.Hash(refreshToken);

        var storedToken = await _refreshTokenRepo.GetByHashAsync(refreshTokenHash);

        if (storedToken is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        if (storedToken.RevokedAt is not null)
        {
            await _refreshTokenRepo.RevokeAllByUserAsync(storedToken.UserID);
            await _uow.SaveChangesAsync();

            throw new UnauthorizedAccessException("Refresh token reuse detected");
        }

        if (!storedToken.IsActive(now))
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = await _userRepo.GetByIdAsync(storedToken.UserID);

        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = TokenHasher.Hash(newRefreshToken);

        await _refreshTokenRepo.RevokeAsync(refreshTokenHash, replacedBy: newRefreshTokenHash);

        await _refreshTokenRepo.AddAsync(new RefreshToken
        {
            UserID = user.UserID,
            TokenHash = newRefreshTokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(_jwt.RefreshTokenDays)
        });

        var newAccessToken = _tokenService.GenerateAccessToken(user);

        await _uow.SaveChangesAsync();

        return new RefreshResponse(newAccessToken, newRefreshToken);
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var refreshTokenHash = TokenHasher.Hash(refreshToken);
        var storedToken = await _refreshTokenRepo.GetActiveAsync(refreshTokenHash);

        if (storedToken is null)
            throw new UnauthorizedAccessException("Token not found");

        await _refreshTokenRepo.RevokeAsync(refreshTokenHash);
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
    public async Task ActivateAccountAsync(ActivateAccountRequest request, CancellationToken ct = default)
    {
        // 1. VALIDATE PASSWORD MATCH
        if (request.NewPassword != request.ConfirmPassword)
            throw new ArgumentException("Mật khẩu xác nhận không khớp");

        if (request.NewPassword.Length < 8)
            throw new ArgumentException("Mật khẩu phải có ít nhất 8 ký tự");

        // 2. LẤY TOKEN TỪ REDIS
        var tokenData = await _activationTokenService.GetAsync(request.Token, ct);
        if (tokenData == null)
            throw new InvalidOperationException("Link kích hoạt không hợp lệ hoặc đã hết hạn");

        // 3. LẤY USER
        // ✅ đúng thứ tự parameter
        var user = await _userRepo.GetByIdAsync(
            tokenData.UserId,
            trackChanges: true,
            include: null,
            ct: ct)
            ?? throw new KeyNotFoundException("Tài khoản không tồn tại");

        if (user.IsActive)
            throw new InvalidOperationException("Tài khoản đã được kích hoạt trước đó");

        // 4. ĐẶT MẬT KHẨU + KÍCH HOẠT
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.IsActive = true;
        user.LastPasswordChange = DateTime.UtcNow;

        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);

        // 5. XÓA TOKEN KHỎI REDIS
        await _activationTokenService.RevokeAsync(request.Token, ct);
    }
}
