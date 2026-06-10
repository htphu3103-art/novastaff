using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.BusinessLayers.Interfaces.Redis
{
    public interface ITokenBlacklistService
    {
        Task BlacklistAsync(string jti, TimeSpan ttl, CancellationToken ct = default);
        Task<bool> IsBlacklistedAsync(string jti, CancellationToken ct = default);

        // 👈 thêm 2 method này
        Task BlacklistUserAsync(int userId, TimeSpan ttl, CancellationToken ct = default);
        Task<bool> IsUserBlacklistedAsync(int userId, CancellationToken ct = default);
    }
}
