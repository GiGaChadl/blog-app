namespace BlogApp.Domain.ValueObjects;

/// <summary>
/// Không phải Entity (không có Id, không lưu bảng riêng) — chỉ là hình dạng dữ liệu
/// để (de)serialize ContactInfo.SocialLinksJson bằng System.Text.Json ở tầng Application.
/// </summary>
public class SocialLink
{
    public string Label { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public string Url { get; set; } = null!;
}
