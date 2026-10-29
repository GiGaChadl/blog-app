namespace BlogApp.Domain.Entities;

/// <summary>Cấu hình SEO toàn site (fallback khi 1 trang không tự đặt SEO riêng).
/// sitemap.xml sinh tự động mỗi khi Posts thay đổi trạng thái công khai.</summary>
public class SeoSettings
{
    public Guid Id { get; set; }
    public string? DefaultMetaTitle { get; set; }
    public string? DefaultMetaDescription { get; set; }
    public string? RobotsTxtContent { get; set; }
    public DateTime? SitemapGeneratedAt { get; set; }
}
