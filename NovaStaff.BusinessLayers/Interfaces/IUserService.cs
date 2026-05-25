using NovaStaff.Models.DTOs.UserAuths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.BusinessLayers.Interfaces
{
    public interface IUserService
    {
        Task<int> CreateAsync(CreateUserRequest request, CancellationToken ct = default);

        Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default);

        Task LockAsync(int userId, CancellationToken ct = default);

        Task UnlockAsync(int userId, CancellationToken ct = default);

        Task UpdateRoleAsync(int userId, NovaStaff.Models.Enums.UserRole role, CancellationToken ct = default);

        Task<UserProfileDto> GetMyProfileAsync(CancellationToken ct = default);

        Task<string> ResetPasswordAsync(int userId, CancellationToken ct = default);
    }
}
