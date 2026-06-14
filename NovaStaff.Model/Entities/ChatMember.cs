using NovaStaff.Models.Common;
using NovaStaff.Models.Enums;

namespace NovaStaff.Models.Entities;

public class ChatMember : BaseEntity
{
    public int ChatMemberID { get; set; }
    public int ChatChannelID { get; set; }
    public int UserID { get; set; }
    public ChatMemberRole Role { get; set; } = ChatMemberRole.Member;
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

    // Lưu thời điểm user đọc tin nhắn cuối — dùng cho unread count
    public DateTimeOffset? LastReadAt { get; set; }

    // Navigation
    public virtual ChatChannel Channel { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
