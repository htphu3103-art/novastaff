namespace NovaStaff.Infrastructure.Email;

public class EmailSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string SenderEmail { get; set; } = "";
    public string SenderName { get; set; } = "";
}