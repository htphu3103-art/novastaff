using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaStaff.Shared.Email;

namespace NovaStaff.Infrastructure.Email;

public class ResendEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly HttpClient _http;

    public ResendEmailService(IOptions<EmailSettings> options, ILogger<ResendEmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var payload = new
        {
            from = $"{_settings.SenderName} <{_settings.SenderEmail}>",
            to = new[] { message.To },
            subject = message.Subject,
            html = message.HtmlBody
        };

        var response = await _http.PostAsJsonAsync("https://api.resend.com/emails", payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Resend API error: {response.StatusCode} - {error}");
        }

        _logger.LogInformation("Resend: gửi email thành công tới {Email}", message.To);
    }
}
