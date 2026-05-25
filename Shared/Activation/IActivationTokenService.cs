namespace NovaStaff.Shared.Activation;

public interface IActivationTokenService
{
    /// <summary>Tạo token và lưu vào Redis, trả về token string</summary>
    Task<string> CreateAsync(ActivationTokenData data, CancellationToken ct = default);

    /// <summary>Lấy data từ token, trả về null nếu không tồn tại hoặc hết hạn</summary>
    Task<ActivationTokenData?> GetAsync(string token, CancellationToken ct = default);

    /// <summary>Xóa token sau khi đã dùng</summary>
    Task RevokeAsync(string token, CancellationToken ct = default);
}

public record ActivationTokenData(
    int UserId,
    string Email,
    string FullName
);