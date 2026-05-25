using NovaStaff.Models.Common;

namespace NovaStaff.Models.Entities;

public class MessageAttachment : BaseEntity
{
    public int MessageAttachmentID { get; set; }
    public int ChatMessageID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;  // relative path trên server
    public string ContentType { get; set; } = string.Empty; // "image/png", "application/pdf"
    public long FileSize { get; set; }  // bytes

    // Navigation
    public virtual ChatMessage Message { get; set; } = null!;
}
