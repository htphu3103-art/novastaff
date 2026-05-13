using NovaStaff.Models.Enums;

namespace NovaStaff.Models.DTOs.UserAuths;

public record CreateUserRequest(
    string Username,
    string Password,
    int EmployeeId,
    UserRole Role
);
