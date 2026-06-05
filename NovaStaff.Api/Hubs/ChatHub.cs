using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.Models.DTOs.Chat;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NovaStaff.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IPresenceTracker _presence;

    public ChatHub(IChatService chatService, IPresenceTracker presence)
    {
        _chatService = chatService;
        _presence = presence;
    }

    // ── Connection lifecycle ──────────────────────────────────

    public override async Task OnConnectedAsync()
    {
        var userID = GetUserID();
        Console.WriteLine($"[ChatHub] Connected: userID={userID}, connID={Context.ConnectionId}");

        await _presence.UserConnected(userID, Context.ConnectionId);

        // Join tất cả SignalR groups tương ứng với channels của user
        var channels = await _chatService.GetChannelsForUserAsync(userID);
        foreach (var ch in channels)
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(ch.ChatChannelID));

        // Thông báo cho những người khác biết user này online
        await Clients.Others.SendAsync("UserOnline", userID);

        // Gửi danh sách user đang online cho chính người vừa kết nối
        var onlineUsers = _presence.GetOnlineUserIDs();
        Console.WriteLine($"[ChatHub] OnlineUsers count: {onlineUsers.Length}");
        
        await Clients.Caller.SendAsync("OnlineUsersList", onlineUsers);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userID = GetUserID();
        await _presence.UserDisconnected(userID, Context.ConnectionId);

        // Chỉ thông báo offline khi KHÔNG còn tab nào khác
        if (!_presence.IsOnline(userID))
            await Clients.Others.SendAsync("UserOffline", userID);

        await base.OnDisconnectedAsync(exception);
    }

    // ── Client → Server methods ───────────────────────────────

    /// <summary>Client gọi khi muốn vào xem 1 channel (focus tab)</summary>
    public async Task JoinChannel(int channelID)
    {
        var userID = GetUserID();
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(channelID));
        await _chatService.MarkChannelReadAsync(channelID, userID);
    }

    /// <summary>Danh sach user dang online.</summary>
    public Task<int[]> GetOnlineUsers()
    {
        return Task.FromResult(_presence.GetOnlineUserIDs());
    }

    /// <summary>Gui tin nhan moi.</summary>
    public async Task SendMessage(int channelID, SendMessageRequest request)
    {
        var userID = GetUserID();

        // Validate + lưu DB
        var messageDto = await _chatService.SaveMessageAsync(channelID, userID, request);

        // Broadcast cho tất cả member trong channel (kể cả người gửi)
        await Clients.Group(GroupName(channelID))
            .SendAsync("ReceiveMessage", messageDto);
    }

    /// <summary>Typing indicator — không lưu DB</summary>
    public async Task Typing(int channelID, bool isTyping)
    {
        var userID = GetUserID();
        // Gửi cho những người khác trong channel, không gửi lại chính mình
        await Clients.OthersInGroup(GroupName(channelID))
            .SendAsync("UserTyping", new { channelID, userID, isTyping });
    }

    /// <summary>React / un-react emoji</summary>
    public async Task ToggleReaction(int messageID, int channelID, string emoji)
    {
        var userID = GetUserID();
        var reaction = await _chatService.ToggleReactionAsync(messageID, userID, emoji);

        await Clients.Group(GroupName(channelID))
            .SendAsync("ReactionUpdated", new { messageID, reaction });
    }

    /// <summary>Xoá tin nhắn</summary>
    public async Task DeleteMessage(int messageID, int channelID)
    {
        var userID = GetUserID();
        await _chatService.DeleteMessageAsync(messageID, userID);

        await Clients.Group(GroupName(channelID))
            .SendAsync("MessageDeleted", messageID);
    }

    // ── Helpers ───────────────────────────────────────────────

    private int GetUserID()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("UserID")?.Value;

        if (!int.TryParse(claim, out var id))
            throw new HubException("Không xác định được user.");

        return id;
    }

    private static string GroupName(int channelID) => $"channel_{channelID}";
}
