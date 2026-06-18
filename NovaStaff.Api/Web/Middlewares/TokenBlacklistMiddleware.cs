// NovaStaff.Web/Middlewares/TokenBlacklistMiddleware.cs
using Microsoft.IdentityModel.Tokens;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.BusinessLayers.Interfaces.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NovaStaff.Web.Middlewares;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public TokenBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenBlacklistService blacklist)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userIdClaim != null && int.TryParse(userIdClaim, out var userId))
        {
            if (await blacklist.IsUserBlacklistedAsync(userId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Tài khoản đã bị khóa." });
                return;
            }
        }

        // Thêm phần này:
        var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (jti != null && await blacklist.IsBlacklistedAsync(jti))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "Phiên đăng nhập đã hết hạn." });
            return;
        }

        await _next(context);
    }
}