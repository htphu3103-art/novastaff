using Microsoft.AspNetCore.Http;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces;
using System.Security.Claims;

namespace NovaStaff.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?
            .User?
            .FindFirst(ClaimTypes.NameIdentifier)?
            .Value;

        return int.TryParse(userId, out var id) ? id : (int?)null;
    }

    public string? GetDisplayName()
    {
        return _httpContextAccessor.HttpContext?.User?
                   .FindFirst(ClaimTypes.Name)?.Value
               ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }

    public string? GetIpAddress()
    {
        return _httpContextAccessor.HttpContext?
            .Connection?
            .RemoteIpAddress?
            .ToString();
    }

    public string? GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?
            .Request?
            .Headers["User-Agent"]
            .ToString();
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    public string? GetRole()
    {
        return _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.Role)?.Value;
    }
}



