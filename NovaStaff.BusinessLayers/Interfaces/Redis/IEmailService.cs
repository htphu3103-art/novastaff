using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ✅ Business/Interfaces/IEmailService.cs
namespace NovaStaff.BusinessLayers.Interfaces.Email  // ← đổi namespace
{
    public interface IEmailService  // ← public, không phải internal
    {
        Task SendAsync(EmailMessage message, CancellationToken ct = default);
    }

    public record EmailMessage(
        string To,
        string Subject,
        string HtmlBody
    );
}
