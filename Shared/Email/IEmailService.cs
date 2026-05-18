namespace NovaStaff.Shared.Email;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}