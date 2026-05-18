namespace NovaStaff.Shared.Email;

public static class EmployeeEmailTemplates
{
    public static EmailMessage Welcome(string toEmail, string fullName,
        string activationLink) => new(
        To: toEmail,
        Subject: "Chào mừng bạn đến NovaStaff!",
        HtmlBody: $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
                <h2 style="color:#2563eb">Chào mừng {fullName}!</h2>
                <p>Tài khoản của bạn đã được tạo bởi Admin.</p>
                <p>Vui lòng click vào link bên dưới để kích hoạt tài khoản và đặt mật khẩu:</p>
                <a href="{activationLink}"
                   style="display:inline-block;padding:12px 24px;background:#2563eb;
                          color:white;border-radius:8px;text-decoration:none;
                          font-weight:bold;margin:16px 0">
                    Kích hoạt tài khoản
                </a>
                <p style="color:#6b7280;font-size:14px">
                    ⚠️ Link có hiệu lực trong <b>48 giờ</b>.
                </p>
            </div>
            """
    );

    public static EmailMessage ResetPassword(string toEmail, string fullName,
        string activationLink) => new(
        To: toEmail,
        Subject: "Reset mật khẩu - NovaStaff",
        HtmlBody: $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
                <h2 style="color:#2563eb">Xin chào {fullName},</h2>
                <p>Mật khẩu của bạn đã được reset bởi Admin.</p>
                <p>Vui lòng click vào link bên dưới để đặt mật khẩu mới:</p>
                <a href="{activationLink}"
                   style="display:inline-block;padding:12px 24px;background:#2563eb;
                          color:white;border-radius:8px;text-decoration:none;
                          font-weight:bold;margin:16px 0">
                    Đặt mật khẩu mới
                </a>
                <p style="color:#6b7280;font-size:14px">
                    ⚠️ Link có hiệu lực trong <b>48 giờ</b>.
                </p>
            </div>
            """
    );
}