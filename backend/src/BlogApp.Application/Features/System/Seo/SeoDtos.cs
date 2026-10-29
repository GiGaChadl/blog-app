namespace BlogApp.Application.Features.System.Seo;

public record SeoSettingsDto(
    string? DefaultMetaTitle,
    string? DefaultMetaDescription,
    string? RobotsTxtContent,
    DateTime? SitemapGeneratedAt,
    string? SiteName = "Nhật Ký Của Tôi",
    string? DefaultTitle = null,
    string? SiteLogoUrl = null,
    string? FaviconUrl = null,
    string? DefaultOgImage = null,
    string? GoogleSiteVerification = null,
    string? DefaultKeywords = null
);

public record UpdateSeoSettingsDto(
    string? DefaultMetaTitle = null,
    string? DefaultMetaDescription = null,
    string? RobotsTxtContent = null,
    string? SiteName = null,
    string? DefaultTitle = null,
    string? SiteLogoUrl = null,
    string? FaviconUrl = null,
    string? DefaultOgImage = null,
    string? GoogleSiteVerification = null,
    string? DefaultKeywords = null
)
{
    public string? ResolvedTitle => !string.IsNullOrWhiteSpace(DefaultMetaTitle) ? DefaultMetaTitle : DefaultTitle;
}
