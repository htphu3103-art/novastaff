// Models/Requests/ActivateAccountRequest.cs
namespace NovaStaff.Models.Requests;

public class ActivateAccountRequest
{
    public string Token { get; set; } = "";
    public string NewPassword { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}