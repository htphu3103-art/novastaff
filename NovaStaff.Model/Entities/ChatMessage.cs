using NovaStaff.Models.Common;
using NovaStaff.Models.Enums;

namespace NovaStaff.Models.Entities;

public class ChatMessage : BaseEntity
{
    public int ChatMessageID { get; set; }
    public int ChatChannelID { get; set; }
    public int SenderUserID { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;

    // Reply thread
    public int? ReplyToMessageID { get; set; }

    // Soft delete (override BaseEntity nếu cần — ẩn nội dung thay vì xoá hẳn)
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public virtual ChatChannel Channel { get; set; } = null!;
    public virtual User Sender { get; set; } = null!;
    public virtual ChatMessage? ReplyToMessage { get; set; }
    public virtual ICollection<ChatMessage> Replies { get; set; } = [];
    public virtual ICollection<MessageReaction> Reactions { get; set; } = [];
    public virtual ICollection<MessageAttachment> Attachments { get; set; } = [];
}
