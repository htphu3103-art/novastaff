using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Models.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; } // Audit trail

        public User User { get; set; } = null!;

        public bool IsExpired(DateTime now) => ExpiresAt <= now;
        public bool IsActive(DateTime now) => RevokedAt is null && !IsExpired(now);
    }
}
