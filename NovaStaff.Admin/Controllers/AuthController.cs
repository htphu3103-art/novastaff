// Controllers/AuthController.cs
using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IDateTimeService _clock;
    private readonly IWebHostEnvironment _env;
    private readonly JwtSettings _jwt;
    private const string RefreshTokenCookieName = "refreshToken";

    public AuthController(
        IAuthService authService,
        IDateTimeService clock,
        IWebHostEnvironment env,
        IOptions<JwtSettings> jwtOptions)
    {
        _authService = authService;
        _clock = clock;
        _env = env;
        _jwt = jwtOptions.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Username and password are required");

        var result = await _authService.LoginAsync(request.Username, request.Password);

        // ✅ Refresh Token → HttpOnly Cookie
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(new { accessToken = result.AccessToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized("Refresh token not found");

        var result = await _authService.RefreshTokenAsync(refreshToken);

        // ✅ Set lại Cookie với refresh token mới (đã rotate)
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(new { accessToken = result.AccessToken });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (!string.IsNullOrWhiteSpace(refreshToken))
            await _authService.RevokeTokenAsync(refreshToken);

        Response.Cookies.Delete(RefreshTokenCookieName);
        return NoContent();
    }

    // ===== HELPER =====
    private void SetRefreshTokenCookie(string token)
    {
        Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment() || Request.IsHttps,
            SameSite = _env.IsDevelopment()
                ? SameSiteMode.Lax
                : SameSiteMode.None,
            Expires = new DateTimeOffset(_clock.UtcNow)
                .AddDays(_jwt.RefreshTokenDays)
        });
    }
}
