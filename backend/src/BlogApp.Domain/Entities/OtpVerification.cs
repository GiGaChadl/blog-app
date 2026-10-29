using BlogApp.Domain.Enums;

namespace BlogApp.Domain.Entities;

public class OtpVerification
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string OtpCodeHash { get; set; } = null!;
    public OtpPurpose Purpose { get; set; }

    /// <summary>
    /// CHỈ có giá trị khi Purpose = ReviewVerification (chống lạm dụng khi bình luận).
    /// Null khi Purpose = AdminPasswordReset. Ép bằng CHECK constraint chk_otp_ip_required_for_review.
    /// </summary>
    public string? IpAddress { get; set; }

    public bool IsUsed { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
