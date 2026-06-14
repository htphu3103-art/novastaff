using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.Services.Interfaces;

namespace NovaStaff.API.Hubs;

[Authorize]
public class AttendanceHub : Hub
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPresenceTracker _presenceTracker;

    public AttendanceHub(
        ICurrentUserService currentUser,
        IPresenceTracker presenceTracker)
    {
        _currentUser = currentUser;
        _presenceTracker = presenceTracker;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _currentUser.GetUserId();

        if (userId == null)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");

        if (Context.User?.IsInRole("HR") == true || Context.User?.IsInRole("Admin") == true)
            await Groups.AddToGroupAsync(Context.ConnectionId, "HR");

        await _presenceTracker.UserConnectedAsync(userId.Value, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUser.GetUserId(); // int?

        if (userId != null)
            await _presenceTracker.UserDisconnectedAsync(userId.Value, Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

}