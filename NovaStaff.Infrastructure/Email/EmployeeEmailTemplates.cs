using NovaStaff.Shared.Email;

namespace NovaStaff.Infrastructure.Email;

public static class EmployeeEmailTemplates
{
    public static EmailMessage Welcome(string toEmail, string fullName,
        string username, string password) => new(
        To: toEmail,
        Subject: "Chào mừng bạn đến NovaStaff!",
        HtmlBody: $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
                <h2 style="color:#2563eb">Chào mừng {fullName}!</h2>
                <p>Tài khoản của bạn đã được tạo. Thông tin đăng nhập:</p>
                <table style="border-collapse:collapse;width:100%">
                    <tr>
                        <td style="padding:8px;border:1px solid #e5e7eb"><b>Tên đăng nhập</b></td>
                        <td style="padding:8px;border:1px solid #e5e7eb">{username}</td>
                    </tr>
                    <tr>
                        <td style="padding:8px;border:1px solid #e5e7eb"><b>Mật khẩu</b></td>
                        <td style="padding:8px;border:1px solid #e5e7eb">{password}</td>
                    </tr>
                </table>
                <p style="color:#dc2626;margin-top:16px">
                    ⚠️ Vui lòng đổi mật khẩu sau lần đăng nhập đầu tiên.
                </p>
            </div>
            """
    );

    public static EmailMessage ResetPassword(string toEmail, string fullName,
        string newPassword) => new(
        To: toEmail,
        Subject: "Reset mật khẩu - NovaStaff",
        HtmlBody: $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
                <h2 style="color:#2563eb">Xin chào {fullName},</h2>
                <p>Mật khẩu của bạn đã được reset bởi Admin:</p>
                <p style="font-size:24px;font-weight:bold;color:#2563eb;
                    padding:16px;background:#eff6ff;border-radius:8px">
                    {newPassword}
                </p>
                <p style="color:#dc2626">
                    ⚠️ Vui lòng đổi mật khẩu sau khi đăng nhập.
                </p>
            </div>
            """
    );
}