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
        Task<RefreshToken?> GetByHashAsync(string tokenHash);
        Task<RefreshToken?> GetActiveAsync(string tokenHash);
        Task RevokeAsync(string tokenHash, string? replacedBy = null);
        Task RevokeAllByUserAsync(int userId, CancellationToken ct = default); 
    }
}
