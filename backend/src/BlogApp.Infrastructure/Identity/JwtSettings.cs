namespace BlogApp.Infrastructure.Identity;

/// <summary>Bind từ appsettings.json section "Jwt". Secret PHẢI đủ dài (>= 32 ký tự) vì dùng
/// thuật toán HMAC-SHA256 — secret ngắn sẽ ném exception lúc tạo token, không phải lỗi ẩn.</summary>
public class JwtSettings
{
    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpiryMinutes { get; set; } = 60;
}
