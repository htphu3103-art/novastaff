using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.Hubs;
using NovaStaff.Models.DTOs.Chat;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NovaStaff.Controllers;

[Authorize]
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _hubContext = hubContext;
    }

    // GET api/chat/channels
    // Optional query param: ?type=Direct or ?type=Group
    [HttpGet("channels")]
    public async Task<IActionResult> GetChannels([FromQuery] string? type = null)
    {
        var userID = GetUserID();
        var channels = await _chatService.GetChannelsForUserAsync(userID);
        
        if (!string.IsNullOrEmpty(type))
        {
            channels = channels.Where(c => c.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        return Ok(channels);
    }

    // GET api/chat/channels/directs
    // Endpoint dĂ nh riĂªng cho FE láº¥y DM list nhanh gá»n
    [HttpGet("channels/directs")]
    public async Task<IActionResult> GetDirects()
    {
        var userID = GetUserID();
        var channels = await _chatService.GetChannelsForUserAsync(userID);
        var directs = channels.Where(c => c.Type == "Direct").ToList();
        return Ok(directs);
    }

    // POST api/chat/channels
    [HttpPost("channels")]
    public async Task<IActionResult> CreateChannel([FromBody] CreateChannelRequest request)
    {
        var userID = GetUserID();
        var channel = await _chatService.CreateGroupChannelAsync(userID, request);
        return Ok(channel);
    }

    // POST api/chat/channels/direct
    [HttpPost("channels/direct")]
    public async Task<IActionResult> CreateDirect([FromBody] CreateDirectRequest request)
    {
        var userID = GetUserID();
        var channel = await _chatService.GetOrCreateDirectChannelAsync(userID, request.TargetUserID);
        return Ok(channel);
    }

    // GET api/chat/channels/{channelID}/messages?pageSize=30&beforeMessageID=100
    [HttpGet("channels/{channelID}/messages")]
    public async Task<IActionResult> GetMessages(
        int channelID,
        [FromQuery] int pageSize = 30,
        [FromQuery] int? beforeMessageID = null)
    {
        var userID = GetUserID();
        try
        {
            var result = await _chatService.GetMessagesAsync(channelID, userID, pageSize, beforeMessageID);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // POST api/chat/channels/{channelID}/messages
    // REST API Ä‘á»ƒ gá»­i tin nháº¯n má»›i (Fallback tá»‘t hÆ¡n gá»i qua SignalR)
    [HttpPost("channels/{channelID}/messages")]
    public async Task<IActionResult> SendMessage(int channelID, [FromBody] SendMessageRequest request)
    {
        var userID = GetUserID();
        try
        {
            // 1. LÆ°u vĂ o Database
            var messageDto = await _chatService.SaveMessageAsync(channelID, userID, request);
            
            // 2. Broadcast qua SignalR cho cĂ¡c thĂ nh viĂªn trong kĂªnh
            await _hubContext.Clients.Group($"channel_{channelID}").SendAsync("ReceiveMessage", messageDto);
            
            return Ok(messageDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET api/chat/channels/{channelID}/members
    [HttpGet("channels/{channelID}/members")]
    public async Task<IActionResult> GetMembers(int channelID)
    {
        var members = await _chatService.GetChannelMembersAsync(channelID);
        return Ok(members);
    }

    // POST api/chat/channels/{channelID}/read
    [HttpPost("channels/{channelID}/read")]
    public async Task<IActionResult> MarkRead(int channelID)
    {
        var userID = GetUserID();
        await _chatService.MarkChannelReadAsync(channelID, userID);
        return NoContent();
    }
    // POST api/chat/channels/{channelID}/members
    [HttpPost("channels/{channelID}/members")]
    public async Task<IActionResult> AddMembers(int channelID, [FromBody] System.Collections.Generic.List<int> memberUserIDs)
    {
        var userID = GetUserID();
        try
        {
            var success = await _chatService.AddMembersToChannelAsync(channelID, userID, memberUserIDs);
            if (!success) return BadRequest("KĂªnh khĂ´ng tá»“n táº¡i hoáº·c khĂ´ng pháº£i kĂªnh nhĂ³m.");
            return Ok(new { message = "ThĂªm thĂ nh viĂªn thĂ nh cĂ´ng." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
    // GET api/chat/users
    [HttpGet("users")]
    public async Task<IActionResult> GetChatUsersLookup()
    {
        var userID = GetUserID();
        var users = await _chatService.GetChatUsersLookupAsync(userID);
        return Ok(users);
    }



    private int GetUserID()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("UserID")?.Value;

        return int.TryParse(claim, out var id) ? id
            : throw new UnauthorizedAccessException();
    }
}


