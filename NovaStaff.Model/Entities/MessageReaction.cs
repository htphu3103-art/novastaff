using NovaStaff.Models.Common;

namespace NovaStaff.Models.Entities;

public class MessageReaction : BaseEntity
{
    public int MessageReactionID { get; set; }
    public int ChatMessageID { get; set; }
    public int UserID { get; set; }
    public string Emoji { get; set; } = string.Empty; // "👍", "❤️", ...

    // Navigation
    public virtual ChatMessage Message { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
