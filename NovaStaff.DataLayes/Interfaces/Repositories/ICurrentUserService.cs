namespace NovaStaff.BusinessLayers.Interfaces;

public interface ICurrentUserService
{
    int? GetUserId();
    string? GetDisplayName();
    string? GetIpAddress();
    string? GetUserAgent();
    string? GetRole();
    bool IsAuthenticated();

    int? GetEmployeeId();
}
