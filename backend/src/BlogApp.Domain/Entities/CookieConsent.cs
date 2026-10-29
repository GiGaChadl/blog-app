using BlogApp.Domain.Enums;

namespace BlogApp.Domain.Entities;

/// <summary>Chỉ ghi dòng khi: (1) bấm Đồng ý lần đầu, hoặc (2) chủ động thu hồi sau này.
/// Bấm Từ chối lần đầu KHÔNG tạo dòng nào.</summary>
public class CookieConsent
{
    public Guid Id { get; set; }
    public string VisitorToken { get; set; } = null!;
    public CookieChoice Choice { get; set; }
    public DateTime ConsentedAt { get; set; }
}
