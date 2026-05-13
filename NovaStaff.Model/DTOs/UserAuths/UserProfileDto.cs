namespace NovaStaff.Models.DTOs.UserAuths;

public record UserProfileDto(
    int UserId,
    string? DisplayName,
    string Role
);
