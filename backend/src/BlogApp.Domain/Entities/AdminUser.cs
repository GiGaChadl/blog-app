namespace BlogApp.Domain.Entities;

/// <summary>Chỉ 1 dòng theo yêu cầu "1 admin duy nhất" — không ép bằng DB, chỉ vì UI không có
/// màn hình tạo thêm admin. Quên mật khẩu tra OtpVerification theo Email + Purpose=AdminPasswordReset.</summary>
public class AdminUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
