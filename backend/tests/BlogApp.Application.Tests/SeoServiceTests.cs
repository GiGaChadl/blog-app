using BlogApp.Application.Features.System.Seo;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class SeoServiceTests
{
    private (TestApplicationDbContext Db, SeoService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var service = new SeoService(db);

        return (db, service);
    }

    [Fact]
    public async Task UpdateSettingsAsync_SavesSeoMetadata()
    {
        var (db, service) = CreateService();

        var dto = new UpdateSeoSettingsDto(
            "Blog Lập Trình .NET & Angular",
            "Mô tả blog chuyên sâu",
            "User-agent: *\nDisallow: /admin"
        );

        var result = await service.UpdateSettingsAsync(dto);

        Assert.Equal("Blog Lập Trình .NET & Angular", result.DefaultMetaTitle);
        Assert.Equal("Mô tả blog chuyên sâu", result.DefaultMetaDescription);
        Assert.Equal("User-agent: *\nDisallow: /admin", result.RobotsTxtContent);

        var saved = await db.SeoSettings.FirstAsync();
        Assert.Equal("Blog Lập Trình .NET & Angular", saved.DefaultMetaTitle);
    }

    [Fact]
    public async Task GetRobotsTxtContentAsync_ReturnsCustomOrStandardContent()
    {
        var (db, service) = CreateService();

        // Ban đầu chưa có trong DB: trả về default
        var defaultRobots = await service.GetRobotsTxtContentAsync("https://myblog.com");
        Assert.Contains("User-agent: *", defaultRobots);
        Assert.Contains("https://myblog.com/sitemap.xml", defaultRobots);

        // Sau khi cấu hình custom
        await service.UpdateSettingsAsync(new UpdateSeoSettingsDto(null, null, "User-agent: Googlebot\nAllow: /"));
        var customRobots = await service.GetRobotsTxtContentAsync("https://myblog.com");
        Assert.Equal("User-agent: Googlebot\nAllow: /", customRobots);
    }

    [Fact]
    public async Task GenerateSitemapXmlAsync_IncludesPublishedPostsAndCategories()
    {
        var (db, service) = CreateService();

        db.Categories.Add(new Category
        {
            Id = Guid.NewGuid(),
            Name = "Lập Trình Web",
            Slug = "lap-trinh-web",
            Level = 3
        });

        db.Posts.AddRange(
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Bài viết 1",
                Slug = "bai-viet-1",
                Status = PostStatus.Published,
                PublishedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Bài viết ẩn",
                Slug = "bai-viet-an",
                Status = PostStatus.Hidden, // Bị ẩn: không được đưa vào sitemap
                PublishedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        var xml = await service.GenerateSitemapXmlAsync("https://myblog.com");

        Assert.Contains("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", xml);
        Assert.Contains("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">", xml);
        Assert.Contains("<loc>https://myblog.com/danh-muc/lap-trinh-web</loc>", xml);
        Assert.Contains("<loc>https://myblog.com/bai-viet/bai-viet-1</loc>", xml);
        Assert.DoesNotContain("bai-viet-an", xml);

        // Kiểm tra mốc SitemapGeneratedAt được cập nhật
        var settings = await db.SeoSettings.FirstOrDefaultAsync();
        Assert.NotNull(settings);
        Assert.NotNull(settings.SitemapGeneratedAt);
    }
}
