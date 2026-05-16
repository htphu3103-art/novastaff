namespace NovaStaff.Models.DTOs.Auth;

public record RefreshResponse(
    string AccessToken,
    string RefreshToken
);
