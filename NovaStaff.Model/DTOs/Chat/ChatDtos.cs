using System;
using System.Collections.Generic;

namespace NovaStaff.Models.DTOs.Chat;

// ── Outbound (Server → Client) ──────────────────────────────

public class ChannelDto
{
    public int ChatChannelID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;   // "Group" | "Direct"
    public int UnreadCount { get; set; }
    public MessageDto? LastMessage { get; set; }
    /// <summary>Chỉ có với DM: UserID của người dùng còn lại trong cuộc trò chuyện.</summary>
    public int? TargetUserID { get; set; }
}

public class MessageDto
{
    public int ChatMessageID { get; set; }
    public int ChatChannelID { get; set; }
    public int SenderUserID { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderInitials { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "Text";
    public int? ReplyToMessageID { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset SentAt { get; set; }
    public List<ReactionDto> Reactions { get; set; } = [];
    public List<AttachmentDto> Attachments { get; set; } = [];
}

public class ReactionDto
{
    public string Emoji { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool ReactedByMe { get; set; }
}

public class AttachmentDto
{
    public int MessageAttachmentID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

public class MemberDto
{
    public int UserID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
}

// ── Inbound (Client → Server) ───────────────────────────────

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public int? ReplyToMessageID { get; set; }
}

public class CreateChannelRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> MemberUserIDs { get; set; } = [];
}

public class CreateDirectRequest
{
    public int TargetUserID { get; set; }
}

// ── Pagination ───────────────────────────────────────────────

public class MessagePageResult
{
    public List<MessageDto> Messages { get; set; } = [];
    public bool HasMore { get; set; }
    public int? NextCursor { get; set; }  // MessageID nhỏ nhất trong page hiện tại
}
