namespace NovaStaff.Models.DTOs.Chat;

public class ChatUserLookupDto
{
    public int UserID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string? Department { get; set; }
}
