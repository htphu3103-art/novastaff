using Microsoft.EntityFrameworkCore;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.DTOs.UserAuths;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;

namespace NovaStaff.BusinessLayers.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public UserService(
            IUserRepository userRepo,
            IUnitOfWork uow,
            ICurrentUserService currentUser)
        {
            _userRepo = userRepo;
            _uow = uow;
            _currentUser = currentUser;
        }
        private string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString("N")[..10]; // đơn giản
        }
        public async Task<int> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
        {
            if (!await _userRepo.IsUsernameUniqueAsync(request.Username, ct: ct))
                throw new InvalidOperationException("Username already exists");

            var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = hash,
                EmployeeID = request.EmployeeId,
                Role = request.Role,
                IsLocked = false,
                FailedLoginAttempts = 0
            };

            await _userRepo.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

            return user.UserID;
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
        {
            var userId = _currentUser.GetUserId()
                ?? throw new UnauthorizedAccessException("Unauthenticated");

            var user = await _userRepo.GetByEmployeeIdAsync(userId, ct: ct)
                ?? throw new KeyNotFoundException("User not found");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid current password");

            var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            await _userRepo.UpdatePasswordAsync(userId, newHash, ct);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task LockAsync(int userId, CancellationToken ct = default)
        {
            var currentUserId = _currentUser.GetUserId();

            if (currentUserId == null)
                throw new UnauthorizedAccessException("Unauthenticated");

            // optional: check role admin ở đây

            await _userRepo.LockUserAsync(userId, DateTime.UtcNow.AddYears(100), ct);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task UnlockAsync(int userId, CancellationToken ct = default)
        {
            await _userRepo.ResetLoginStateAsync(userId, ct);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task UpdateRoleAsync(int userId, NovaStaff.Models.Enums.UserRole role, CancellationToken ct = default)
        {
            var currentUserId = _currentUser.GetUserId()
                ?? throw new UnauthorizedAccessException("Unauthenticated");

            var currentRole = _currentUser.GetRole();

            if (!Enum.TryParse<NovaStaff.Models.Enums.UserRole>(currentRole, out var parsedRole) || parsedRole != NovaStaff.Models.Enums.UserRole.Admin)
                throw new UnauthorizedAccessException("Forbidden");

            var user = await _userRepo.GetByEmployeeIdAsync(userId, ct: ct);

            if (user == null)
            {
                user = new User
                {
                    EmployeeID = userId,
                    Username = $"user{userId}", // hoặc để trống, tùy yêu cầu
                    PasswordHash = string.Empty,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                };

                await _userRepo.AddAsync(user, ct);
            }

            user.Role = role;

            await _uow.SaveChangesAsync(ct);
        }

        public async Task<UserProfileDto> GetMyProfileAsync(CancellationToken ct = default)
        {
            var userId = _currentUser.GetUserId()
                ?? throw new UnauthorizedAccessException("Unauthenticated");

            var user = await _userRepo.GetByIdAsync(
                userId,
                trackChanges: false,
                include: q => q.Include(x => x.Employee),
                ct: ct
            ) ?? throw new KeyNotFoundException("User not found");

            return new UserProfileDto(
                user.UserID,
                user.Employee?.FullName,
                user.Role.ToString()
            );
        }
        public async Task<string> ResetPasswordAsync(int employeeId, CancellationToken ct = default)
        {
            var currentUserId = _currentUser.GetUserId()
                ?? throw new UnauthorizedAccessException("Unauthenticated");

            var currentRole = _currentUser.GetRole();

            if (!Enum.TryParse<UserRole>(currentRole, out var role) || role != UserRole.Admin)
                throw new UnauthorizedAccessException("Forbidden");

            var user = await _userRepo.GetByEmployeeIdAsync(employeeId, ct: ct);

            // 👉 nếu chưa có user thì tạo mới
            if (user == null)
            {
                user = new User
                {
                    EmployeeID = employeeId,
                    Username = $"user{employeeId}", // hoặc để trống, tùy yêu cầu
                    PasswordHash = string.Empty,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                };

                await _userRepo.AddAsync(user, ct);
            }

            var newPassword = GenerateRandomPassword();
            var hash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _userRepo.UpdatePasswordAsync(user.UserID, hash, ct);
            await _uow.SaveChangesAsync(ct);

            return newPassword;
        }
    }
}
