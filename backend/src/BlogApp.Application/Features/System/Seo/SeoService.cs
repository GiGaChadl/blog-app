using global::System.Text;
using JsonNode = global::System.Text.Json.Nodes.JsonNode;
using JsonObject = global::System.Text.Json.Nodes.JsonObject;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.System.Seo;

public class SeoService
{
    private readonly IApplicationDbContext _db;

    public SeoService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<SeoSettingsDto> GetSettingsAsync(CancellationToken ct = default)
    {
        var settings = await _db.SeoSettings.FirstOrDefaultAsync(ct);
        var contact = await _db.ContactInfo.AsNoTracking().FirstOrDefaultAsync(ct);

        string? siteName = "Nhật Ký Của Tôi";
        string? siteLogoUrl = null;
        string? faviconUrl = null;
        string? defaultOgImage = null;
        string? googleSiteVerification = null;
        string? defaultKeywords = null;

        if (contact != null && !string.IsNullOrWhiteSpace(contact.SocialLinksJson))
        {
            try
            {
                var node = JsonNode.Parse(contact.SocialLinksJson)?.AsObject();
                if (node != null)
                {
                    if (node.TryGetPropertyValue("siteName", out var sn) && sn != null) siteName = sn.ToString();
                    if (node.TryGetPropertyValue("siteLogoUrl", out var lu) && lu != null) siteLogoUrl = lu.ToString();
                    if (node.TryGetPropertyValue("faviconUrl", out var fu) && fu != null) faviconUrl = fu.ToString();
                    if (node.TryGetPropertyValue("defaultOgImage", out var og) && og != null) defaultOgImage = og.ToString();
                    if (node.TryGetPropertyValue("googleSiteVerification", out var gv) && gv != null) googleSiteVerification = gv.ToString();
                    if (node.TryGetPropertyValue("defaultKeywords", out var dk) && dk != null) defaultKeywords = dk.ToString();
                }
            }
            catch { }
        }

        var metaTitle = settings?.DefaultMetaTitle ?? "Blog Cá Nhân — Chia sẻ kiến thức lập trình & công nghệ";
        var metaDesc = settings?.DefaultMetaDescription ?? "Blog cá nhân chia sẻ bài viết chuyên sâu về kiến trúc phần mềm, .NET và phát triển web hiện đại.";

        return new SeoSettingsDto(
            DefaultMetaTitle: metaTitle,
            DefaultMetaDescription: metaDesc,
            RobotsTxtContent: settings?.RobotsTxtContent,
            SitemapGeneratedAt: settings?.SitemapGeneratedAt,
            SiteName: siteName,
            DefaultTitle: metaTitle,
            SiteLogoUrl: siteLogoUrl,
            FaviconUrl: faviconUrl,
            DefaultOgImage: defaultOgImage,
            GoogleSiteVerification: googleSiteVerification,
            DefaultKeywords: defaultKeywords
        );
    }

    public async Task<SeoSettingsDto> UpdateSettingsAsync(UpdateSeoSettingsDto dto, CancellationToken ct = default)
    {
        var resolvedTitle = dto.ResolvedTitle?.Trim();
        var settings = await _db.SeoSettings.FirstOrDefaultAsync(ct);
        if (settings == null)
        {
            settings = new SeoSettings
            {
                Id = Guid.NewGuid(),
                DefaultMetaTitle = resolvedTitle,
                DefaultMetaDescription = dto.DefaultMetaDescription?.Trim(),
                RobotsTxtContent = dto.RobotsTxtContent?.Trim(),
                SitemapGeneratedAt = null
            };
            _db.SeoSettings.Add(settings);
        }
        else
        {
            if (resolvedTitle != null) settings.DefaultMetaTitle = resolvedTitle;
            if (dto.DefaultMetaDescription != null) settings.DefaultMetaDescription = dto.DefaultMetaDescription.Trim();
            if (dto.RobotsTxtContent != null) settings.RobotsTxtContent = dto.RobotsTxtContent.Trim();
        }

        // Lưu siteName, siteLogoUrl, faviconUrl, v.v. vào ContactInfo JSON metadata
        var contact = await _db.ContactInfo.FirstOrDefaultAsync(ct);
        if (contact != null)
        {
            try
            {
                var node = !string.IsNullOrWhiteSpace(contact.SocialLinksJson)
                    ? JsonNode.Parse(contact.SocialLinksJson)?.AsObject() ?? new JsonObject()
                    : new JsonObject();

                if (!string.IsNullOrWhiteSpace(dto.SiteName)) node["siteName"] = dto.SiteName.Trim();
                if (dto.SiteLogoUrl != null) node["siteLogoUrl"] = dto.SiteLogoUrl.Trim();
                if (dto.FaviconUrl != null) node["faviconUrl"] = dto.FaviconUrl.Trim();
                if (dto.DefaultOgImage != null) node["defaultOgImage"] = dto.DefaultOgImage.Trim();
                if (dto.GoogleSiteVerification != null) node["googleSiteVerification"] = dto.GoogleSiteVerification.Trim();
                if (dto.DefaultKeywords != null) node["defaultKeywords"] = dto.DefaultKeywords.Trim();

                contact.SocialLinksJson = node.ToJsonString();
                contact.UpdatedAt = DateTime.UtcNow;
            }
            catch { }
        }

        await _db.SaveChangesAsync(ct);
        return await GetSettingsAsync(ct);
    }

    /// <summary>
    /// Lấy nội dung robots.txt (nếu trong DB chưa có thì trả về cấu hình mặc định chuẩn SEO).
    /// </summary>
    public async Task<string> GetRobotsTxtContentAsync(string baseUrl, CancellationToken ct = default)
    {
        var settings = await _db.SeoSettings.FirstOrDefaultAsync(ct);
        if (settings != null && !string.IsNullOrWhiteSpace(settings.RobotsTxtContent))
        {
            return settings.RobotsTxtContent;
        }

        var normalizedBaseUrl = baseUrl.TrimEnd('/');
        return $"User-agent: *\nAllow: /\nDisallow: /api/admin/\nDisallow: /admin/\nSitemap: {normalizedBaseUrl}/sitemap.xml\n";
    }

    /// <summary>
    /// Tự động sinh nội dung sitemap.xml chuẩn Google/Bing URLset từ toàn bộ danh mục và bài viết đang Published.
    /// </summary>
    public async Task<string> GenerateSitemapXmlAsync(string baseUrl, CancellationToken ct = default)
    {
        var normalizedBaseUrl = baseUrl.TrimEnd('/');

        // Lấy toàn bộ bài viết đã xuất bản
        var posts = await _db.Posts
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedAt)
            .Select(p => new { p.Slug, p.UpdatedAt })
            .ToListAsync(ct);

        // Lấy toàn bộ danh mục
        var categories = await _db.Categories
            .AsNoTracking()
            .Select(c => new { c.Slug })
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // Trang chủ
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{normalizedBaseUrl}/</loc>");
        sb.AppendLine("    <changefreq>daily</changefreq>");
        sb.AppendLine("    <priority>1.0</priority>");
        sb.AppendLine("  </url>");

        // Danh mục
        foreach (var cat in categories)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{normalizedBaseUrl}/danh-muc/{cat.Slug}</loc>");
            sb.AppendLine("    <changefreq>weekly</changefreq>");
            sb.AppendLine("    <priority>0.8</priority>");
            sb.AppendLine("  </url>");
        }

        // Bài viết
        foreach (var post in posts)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{normalizedBaseUrl}/bai-viet/{post.Slug}</loc>");
            sb.AppendLine($"    <lastmod>{post.UpdatedAt:yyyy-MM-dd}</lastmod>");
            sb.AppendLine("    <changefreq>monthly</changefreq>");
            sb.AppendLine("    <priority>0.9</priority>");
            sb.AppendLine("  </url>");
        }

        sb.AppendLine("</urlset>");

        // Cập nhật mốc thời gian sinh sitemap vào DB
        var settings = await _db.SeoSettings.FirstOrDefaultAsync(ct);
        if (settings == null)
        {
            settings = new SeoSettings
            {
                Id = Guid.NewGuid(),
                SitemapGeneratedAt = DateTime.UtcNow
            };
            _db.SeoSettings.Add(settings);
        }
        else
        {
            settings.SitemapGeneratedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);

        return sb.ToString();
    }
}
