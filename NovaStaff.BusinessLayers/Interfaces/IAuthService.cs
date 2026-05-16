using NovaStaff.Models.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.BusinessLayers.Interfaces
{
    // Services/Interfaces/IAuthService.cs
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string email, string password);
        Task<RefreshResponse> RefreshTokenAsync(string refreshToken);
        Task RevokeTokenAsync(string refreshToken);          // logout
    }
}
