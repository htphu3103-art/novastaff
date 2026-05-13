namespace NovaStaff.Models.DTOs.UserAuths;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
