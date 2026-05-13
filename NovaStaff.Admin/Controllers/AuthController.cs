// Controllers/AuthController.cs
using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.Models.DTOs.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private const string RefreshTokenCookieName = "refreshToken";

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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

        var newAccessToken = await _authService.RefreshTokenAsync(refreshToken);

        // ✅ Set lại Cookie với refresh token mới (đã rotate)
        // Cần refactor RefreshTokenAsync trả về cả 2, tạm thời đơn giản hóa
        return Ok(new { accessToken = newAccessToken });
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
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }
}