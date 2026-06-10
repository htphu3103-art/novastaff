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
        // Log tất cả claims
        foreach (var claim in context.User.Claims)
            Console.WriteLine($"[Claim] {claim.Type} = {claim.Value}");

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        Console.WriteLine($"[Blacklist] userIdClaim = {userIdClaim}");

        if (userIdClaim != null && int.TryParse(userIdClaim, out var userId))
        {
            var isBlacklisted = await blacklist.IsUserBlacklistedAsync(userId);
            Console.WriteLine($"[Blacklist] userId={userId} isBlacklisted={isBlacklisted}");

            if (isBlacklisted)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Tài khoản đã bị khóa." });
                return;
            }
        }

        await _next(context);
    }
}