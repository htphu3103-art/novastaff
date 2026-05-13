using NovaStaff.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.DataLayers.Interfaces.Repositories
{
    // DataLayers/Interfaces/Repositories/IRefreshTokenRepository.cs
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetActiveAsync(string token);
        Task RevokeAsync(string token, string? replacedBy = null);
        Task RevokeAllByUserAsync(int userId); // Logout tất cả thiết bị
    }
}
