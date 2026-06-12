using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.Models.DTOs.Chat;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;
namespace NovaStaff.BusinessLayers.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _db;
    private readonly IPresenceTracker _presence;
    private readonly IDateTimeService _dateTimeService;

    public ChatService(AppDbContext db, IPresenceTracker presence, IDateTimeService dateTimeService)
    {
        _db = db;
        _presence = presence;
        _dateTimeService = dateTimeService;
    }

    // Ă¢â€â‚¬Ă¢â€â‚¬ Channels Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    public async Task<List<ChannelDto>> GetChannelsForUserAsync(int userID)
    {
        var channels = await _db.ChatMembers
            .Where(m => m.UserID == userID)
            .Include(m => m.Channel)
                .ThenInclude(c => c.Messages.OrderByDescending(msg => msg.CreatedDate).Take(1))
            .Include(m => m.Channel)
                .ThenInclude(c => c.Members)
            .Select(m => new
            {
                m.Channel,
                m.LastReadAt,
                UnreadCount = m.Channel.Messages.Count(msg =>
                    !msg.IsDeleted &&
                    (m.LastReadAt == null || msg.CreatedDate > m.LastReadAt))
            })
            .ToListAsync();

        return channels.Select(x => new ChannelDto
        {
            ChatChannelID = x.Channel.ChatChannelID,
            Name = x.Channel.Name,
            Description = x.Channel.Description,
            Type = x.Channel.Type.ToString(),
            UnreadCount = x.UnreadCount,
            LastMessage = x.Channel.Messages
                .OrderByDescending(m => m.CreatedDate)
                .Select(m => MapToMessageDto(m, userID))
                .FirstOrDefault(),
            // Với DM: gán userID của người còn lại (không phải currentUser)
            TargetUserID = x.Channel.Type == Models.Enums.ChatChannelType.Direct
                ? x.Channel.Members.FirstOrDefault(m => m.UserID != userID)?.UserID
                : null
        }).ToList();
    }

    public async Task<ChannelDto> CreateGroupChannelAsync(int creatorUserID, CreateChannelRequest request)
    {
        var channel = new ChatChannel
        {
            Name = request.Name,
            Description = request.Description,
            Type = ChatChannelType.Group,
        };
        _db.ChatChannels.Add(channel);
        await _db.SaveChangesAsync();

        // ThÄ‚Âªm creator + members
        var allUserIDs = request.MemberUserIDs
            .Append(creatorUserID)
            .Distinct()
            .ToList();

        var members = allUserIDs.Select(uid => new ChatMember
        {
            ChatChannelID = channel.ChatChannelID,
            UserID = uid,
            Role = uid == creatorUserID ? ChatMemberRole.Admin : ChatMemberRole.Member,
            JoinedAt = _dateTimeService.UtcNow
        });

        _db.ChatMembers.AddRange(members);
        await _db.SaveChangesAsync();

        return new ChannelDto
        {
            ChatChannelID = channel.ChatChannelID,
            Name = channel.Name,
            Description = channel.Description,
            Type = channel.Type.ToString(),
        };
    }

    public async Task<ChannelDto> GetOrCreateDirectChannelAsync(int userID, int targetUserID)
    {
        // TÄ‚Â¬m DM channel Ă„â€˜Ä‚Â£ tĂ¡Â»â€œn tĂ¡ÂºÂ¡i giĂ¡Â»Â¯a 2 user
        var existing = await _db.ChatChannels
            .Where(c => c.Type == ChatChannelType.Direct &&
                        c.Members.Any(m => m.UserID == userID) &&
                        c.Members.Any(m => m.UserID == targetUserID) &&
                        c.Members.Count == 2)
            .FirstOrDefaultAsync();

        if (existing != null)
            return new ChannelDto { ChatChannelID = existing.ChatChannelID, Name = existing.Name, Type = "Direct" };

        // LĂ¡ÂºÂ¥y tÄ‚Âªn employee Ă„â€˜Ă¡Â»Æ’ Ă„â€˜Ă¡ÂºÂ·t tÄ‚Âªn channel
        var targetEmployee = await _db.Users
            .Where(u => u.UserID == targetUserID)
            .Select(u => u.Employee!.FullName)
            .FirstOrDefaultAsync() ?? "Unknown";

        var channel = new ChatChannel
        {
            Name = $"DM-{userID}-{targetUserID}",
            Type = ChatChannelType.Direct
        };
        _db.ChatChannels.Add(channel);
        await _db.SaveChangesAsync();

        var now = _dateTimeService.UtcNow;

        _db.ChatMembers.AddRange([
            new ChatMember
    {
        ChatChannelID = channel.ChatChannelID,
        UserID = userID,
        JoinedAt = now
    },
    new ChatMember
    {
        ChatChannelID = channel.ChatChannelID,
        UserID = targetUserID,
        JoinedAt = now
    }
        ]);
        await _db.SaveChangesAsync();

        return new ChannelDto { ChatChannelID = channel.ChatChannelID, Name = targetEmployee, Type = "Direct" };
    }

    
    public async Task<MessagePageResult> GetMessagesAsync(
        int channelID, int userID, int pageSize = 30, int? beforeMessageID = null)
    {
        // KiĂ¡Â»Æ’m tra user cÄ‚Â³ trong channel khÄ‚Â´ng
        var isMember = await _db.ChatMembers
            .AnyAsync(m => m.ChatChannelID == channelID && m.UserID == userID);
        if (!isMember) throw new UnauthorizedAccessException("BĂ¡ÂºÂ¡n khÄ‚Â´ng cÄ‚Â³ trong kÄ‚Âªnh nÄ‚Â y.");

        var query = _db.ChatMessages
            .Where(m => m.ChatChannelID == channelID)
            .Include(m => m.Sender).ThenInclude(u => u.Employee)
            .Include(m => m.Reactions).ThenInclude(r => r.User)
            .Include(m => m.Attachments)
            .AsQueryable();

        if (beforeMessageID.HasValue)
            query = query.Where(m => m.ChatMessageID < beforeMessageID.Value);

        var messages = await query
            .OrderByDescending(m => m.ChatMessageID)
            .Take(pageSize + 1)
            .ToListAsync();

        var hasMore = messages.Count > pageSize;
        if (hasMore) messages.RemoveAt(messages.Count - 1);

        messages.Reverse();

        return new MessagePageResult
        {
            Messages = messages.Select(m => MapToMessageDto(m, userID)).ToList(),
            HasMore = hasMore,
            NextCursor = hasMore ? messages.First().ChatMessageID : null
        };
    }

    public async Task<MessageDto> SaveMessageAsync(
        int channelID, int senderUserID, SendMessageRequest request)
    {
        var message = new ChatMessage
        {
            ChatChannelID = channelID,
            SenderUserID = senderUserID,
            Content = request.Content,
            ReplyToMessageID = request.ReplyToMessageID,
            Type = MessageType.Text,
        };

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync();

        var saved = await _db.ChatMessages
            .Include(m => m.Sender).ThenInclude(u => u.Employee)
            .Include(m => m.Reactions)
            .Include(m => m.Attachments)
            .FirstAsync(m => m.ChatMessageID == message.ChatMessageID);

        return MapToMessageDto(saved, senderUserID);
    }

    public async Task<bool> DeleteMessageAsync(int messageID, int requestingUserID)
    {
        var message = await _db.ChatMessages.FindAsync(messageID);
        if (message == null) return false;

        // ChĂ¡Â»â€° ngĂ†Â°Ă¡Â»Âi gĂ¡Â»Â­i mĂ¡Â»â€ºi Ă„â€˜Ă†Â°Ă¡Â»Â£c xoÄ‚Â¡
        if (message.SenderUserID != requestingUserID)
            throw new UnauthorizedAccessException("BĂ¡ÂºÂ¡n khÄ‚Â´ng cÄ‚Â³ quyĂ¡Â»Ân xoÄ‚Â¡ tin nhĂ¡ÂºÂ¯n nÄ‚Â y.");

        message.IsDeleted = true;
        message.DeletedAt = _dateTimeService.UtcNow;
        message.Content = "[Tin nhĂ¡ÂºÂ¯n Ă„â€˜Ä‚Â£ bĂ¡Â»â€¹ xoÄ‚Â¡]";
        await _db.SaveChangesAsync();
        return true;
    }

    // Ă¢â€â‚¬Ă¢â€â‚¬ Read tracking Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    public async Task MarkChannelReadAsync(int channelID, int userID)
    {
        var member = await _db.ChatMembers
            .FirstOrDefaultAsync(m => m.ChatChannelID == channelID && m.UserID == userID);
        if (member == null) return;

        // Use database time when possible to avoid clock skew
        // (common in Docker where app/db containers may drift).
        var provider = _db.Database.ProviderName ?? string.Empty;
        if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE ""ChatMembers""
SET ""LastReadAt"" = NOW()
WHERE ""ChatChannelID"" = {channelID} AND ""UserID"" = {userID};
");
            return;
        }

        member.LastReadAt = _dateTimeService.UtcNow;
        await _db.SaveChangesAsync();
    }

    // Ă¢â€â‚¬Ă¢â€â‚¬ Reactions Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    public async Task<ReactionDto> ToggleReactionAsync(int messageID, int userID, string emoji)
    {
        var existing = await _db.MessageReactions
            .FirstOrDefaultAsync(r => r.ChatMessageID == messageID && r.UserID == userID && r.Emoji == emoji);

        if (existing != null)
        {
            _db.MessageReactions.Remove(existing);
        }
        else
        {
            _db.MessageReactions.Add(new MessageReaction
            {
                ChatMessageID = messageID,
                UserID = userID,
                Emoji = emoji
            });
        }

        await _db.SaveChangesAsync();

        var count = await _db.MessageReactions
            .CountAsync(r => r.ChatMessageID == messageID && r.Emoji == emoji);

        return new ReactionDto { Emoji = emoji, Count = count, ReactedByMe = existing == null };
    }

    // Ă¢â€â‚¬Ă¢â€â‚¬ Members Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    public async Task<List<MemberDto>> GetChannelMembersAsync(int channelID)
    {
        var members = await _db.ChatMembers
            .Where(m => m.ChatChannelID == channelID)
            .Include(m => m.User).ThenInclude(u => u.Employee)
            .Select(m => new MemberDto
            {
                UserID = m.UserID,
                FullName = m.User.Employee != null ? m.User.Employee.FullName : m.User.Username,
                Initials = BuildInitials(m.User.Employee != null ? m.User.Employee.FullName : m.User.Username),
                IsOnline = false // sĂ¡ÂºÂ½ override tĂ¡Â»Â« PresenceTracker cĂ¡Â»Â§a SignalR
            })
            .ToListAsync();

        foreach (var member in members)
        {
            member.IsOnline = await _presence.IsOnlineAsync(member.UserID);
        }

        return members;
    }

    // Ă¢â€â‚¬Ă¢â€â‚¬ Helpers Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    private static MessageDto MapToMessageDto(ChatMessage m, int currentUserID)
    {
        var fullName = m.Sender?.Employee?.FullName ?? m.Sender?.Username ?? "Unknown";
        return new MessageDto
        {
            ChatMessageID = m.ChatMessageID,
            ChatChannelID = m.ChatChannelID,
            SenderUserID = m.SenderUserID,
            SenderName = fullName,
            SenderInitials = BuildInitials(fullName),
            Content = m.Content,
            Type = m.Type.ToString(),
            ReplyToMessageID = m.ReplyToMessageID,
            IsDeleted = m.IsDeleted,
            SentAt = m.CreatedDate,
            Reactions = m.Reactions
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionDto
                {
                    Emoji = g.Key,
                    Count = g.Count(),
                    ReactedByMe = g.Any(r => r.UserID == currentUserID)
                }).ToList(),
            Attachments = m.Attachments.Select(a => new AttachmentDto
            {
                MessageAttachmentID = a.MessageAttachmentID,
                FileName = a.FileName,
                Url = $"/api/chat/attachments/{a.MessageAttachmentID}",
                ContentType = a.ContentType,
                FileSize = a.FileSize
            }).ToList()
        };
    }

    private static string BuildInitials(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
            : fullName.Length >= 2 ? fullName[..2].ToUpper() : fullName.ToUpper();
    }

    public async Task<bool> AddMembersToChannelAsync(int channelID, int requestingUserID, List<int> newUserIDs)
    {
        var channel = await _db.ChatChannels.Include(c => c.Members).FirstOrDefaultAsync(c => c.ChatChannelID == channelID);
        if (channel == null || channel.Type != ChatChannelType.Group) return false;

        var reqMember = channel.Members.FirstOrDefault(m => m.UserID == requestingUserID);
        if (reqMember == null || reqMember.Role != ChatMemberRole.Admin) 
            throw new UnauthorizedAccessException("ChĂ¡Â»â€° cÄ‚Â³ Admin cĂ¡Â»Â§a kÄ‚Âªnh mĂ¡Â»â€ºi Ă„â€˜Ă†Â°Ă¡Â»Â£c thÄ‚Âªm thÄ‚Â nh viÄ‚Âªn.");

        var existingUserIDs = channel.Members.Select(m => m.UserID).ToHashSet();
        var toAdd = newUserIDs.Where(id => !existingUserIDs.Contains(id)).Distinct().ToList();

        if (toAdd.Count == 0) return true;

        foreach (var uid in toAdd)
        {
            _db.ChatMembers.Add(new ChatMember
            {
                ChatChannelID = channelID,
                UserID = uid,
                Role = ChatMemberRole.Member
            });
        }
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<ChatUserLookupDto>> GetChatUsersLookupAsync(int currentUserID)
    {
        var users = await _db.Users
            .Include(u => u.Employee)
                .ThenInclude(e => e!.Department)
            .Where(u => u.IsActive && u.UserID != currentUserID)
            .Select(u => new ChatUserLookupDto
            {
                UserID = u.UserID,
                FullName = u.Employee != null ? u.Employee.FullName : u.Username,
                Department = u.Employee != null && u.Employee.Department != null ? u.Employee.Department.DepartmentName : null
            })
            .ToListAsync();
            
        foreach(var u in users)
        {
            u.Initials = BuildInitials(u.FullName ?? "Unknown");
        }
        
        return users;
    }
}

