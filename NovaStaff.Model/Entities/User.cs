using NovaStaff.Models.Common;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Entities;
public class User : BaseEntity
{
    public int UserID { get; set; }
    public int EmployeeID { get; set; }

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Staff;

    // ?? Security
    public bool IsLocked { get; set; }
    public int FailedLoginAttempts { get; set; }

    // ?? Thêm DUY NH?T 1 field (r?t ðáng giá)
    public DateTime? LockoutEnd { get; set; }

    // ?? Tracking
    public DateTime? LastLogin { get; set; }
    public DateTime? LastPasswordChange { get; set; }

    public virtual Employee? Employee { get; set; } = null;
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}




