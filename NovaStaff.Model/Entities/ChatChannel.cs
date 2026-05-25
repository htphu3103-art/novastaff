using NovaStaff.Models.Common;
using NovaStaff.Models.Enums;

namespace NovaStaff.Models.Entities;

public class ChatChannel : BaseEntity
{
    public int ChatChannelID { get; set; }
    public string Name { get; set; } = string.Empty;
    public ChatChannelType Type { get; set; } = ChatChannelType.Group;
    public string? Description { get; set; }
    public bool IsArchived { get; set; } = false;

    // Navigation
    public virtual ICollection<ChatMember> Members { get; set; } = [];
    public virtual ICollection<ChatMessage> Messages { get; set; } = [];
}
