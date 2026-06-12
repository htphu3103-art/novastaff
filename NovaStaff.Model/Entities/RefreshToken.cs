public class RefreshToken
{
    public int Id { get; set; }
    public int UserID { get; set; }
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public User User { get; set; } = null!;

    public bool IsExpired(DateTimeOffset now)
        => ExpiresAt <= now;

    public bool IsActive(DateTimeOffset now)
        => RevokedAt is null && !IsExpired(now);
}