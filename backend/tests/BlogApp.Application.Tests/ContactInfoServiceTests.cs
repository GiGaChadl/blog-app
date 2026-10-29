using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Features.Administration.Contact;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class ContactInfoServiceTests
{
    private (TestApplicationDbContext Db, ContactInfoService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var service = new ContactInfoService(db);

        return (db, service);
    }

    [Fact]
    public async Task GetContactInfoAsync_EmptyTable_ReturnsDefaultInfo()
    {
        var (_, service) = CreateService();

        var info = await service.GetContactInfoAsync();
        Assert.NotNull(info);
        Assert.NotEmpty(info.Email);
        Assert.Empty(info.SocialLinks);
    }

    [Fact]
    public async Task UpdateContactInfoAsync_ValidData_SavesAndParsesJsonCorrectly()
    {
        var (db, service) = CreateService();

        var links = new List<SocialLinkDto>
        {
            new("github", "https://github.com/my-profile", "pi-github"),
            new("linkedin", "https://linkedin.com/in/my-profile", "pi-linkedin")
        };
        var updateDto = new UpdateContactInfoDto("contact@myblog.com", "Hà Nội, Việt Nam", links);

        var result = await service.UpdateContactInfoAsync(updateDto);

        Assert.Equal("contact@myblog.com", result.Email);
        Assert.Equal("Hà Nội, Việt Nam", result.Location);
        Assert.Equal(2, result.SocialLinks.Count);
        Assert.Equal("github", result.SocialLinks[0].Platform);

        // Kiểm tra trong DB
        var count = await db.ContactInfo.CountAsync();
        Assert.Equal(1, count);

        // Gọi lại GetContactInfoAsync để xác nhận parse JSONB
        var loaded = await service.GetContactInfoAsync();
        Assert.Equal("contact@myblog.com", loaded.Email);
        Assert.Equal(2, loaded.SocialLinks.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    public async Task UpdateContactInfoAsync_InvalidEmail_ThrowsValidationException(string email)
    {
        var (_, service) = CreateService();

        var updateDto = new UpdateContactInfoDto(email, "HN", null);
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateContactInfoAsync(updateDto));
    }

    [Fact]
    public async Task UpdateContactInfoAsync_ExtendedFields_SavesAndRetrievesPhoneBioAddressAndSocialLinks()
    {
        var (db, service) = CreateService();

        var updateDto = new UpdateContactInfoDto(
            Email: "author@myblog.com",
            Address: "123 Đường Láng, Hà Nội",
            Phone: "0912345678",
            Bio: "Kỹ sư phần mềm đam mê chia sẻ kiến thức.",
            FacebookUrl: "https://facebook.com/author",
            GithubUrl: "https://github.com/author",
            LinkedinUrl: "https://linkedin.com/in/author",
            YoutubeUrl: "https://youtube.com/@author"
        );

        var result = await service.UpdateContactInfoAsync(updateDto);

        Assert.Equal("author@myblog.com", result.Email);
        Assert.Equal("123 Đường Láng, Hà Nội", result.Address);
        Assert.Equal("123 Đường Láng, Hà Nội", result.Location);
        Assert.Equal("0912345678", result.Phone);
        Assert.Equal("Kỹ sư phần mềm đam mê chia sẻ kiến thức.", result.Bio);
        Assert.Equal("https://facebook.com/author", result.FacebookUrl);
        Assert.Equal("https://github.com/author", result.GithubUrl);
        Assert.Equal("https://linkedin.com/in/author", result.LinkedinUrl);
        Assert.Equal("https://youtube.com/@author", result.YoutubeUrl);
        Assert.Equal(4, result.SocialLinks.Count);

        var loaded = await service.GetContactInfoAsync();
        Assert.Equal("author@myblog.com", loaded.Email);
        Assert.Equal("123 Đường Láng, Hà Nội", loaded.Address);
        Assert.Equal("0912345678", loaded.Phone);
        Assert.Equal("Kỹ sư phần mềm đam mê chia sẻ kiến thức.", loaded.Bio);
        Assert.Equal("https://facebook.com/author", loaded.FacebookUrl);
        Assert.Equal("https://github.com/author", loaded.GithubUrl);
        Assert.Equal("https://linkedin.com/in/author", loaded.LinkedinUrl);
        Assert.Equal("https://youtube.com/@author", loaded.YoutubeUrl);
        Assert.Equal(4, loaded.SocialLinks.Count);
    }

    [Fact]
    public async Task GetContactInfoAsync_LegacySocialLinksJsonArray_ParsesWithoutError()
    {
        var (db, service) = CreateService();

        var legacyInfo = new BlogApp.Domain.Entities.ContactInfo
        {
            Id = Guid.NewGuid(),
            Email = "legacy@blog.com",
            Location = "Đà Nẵng",
            SocialLinksJson = "[{\"Platform\":\"github\",\"Url\":\"https://github.com/legacy\"},{\"Platform\":\"facebook\",\"Url\":\"https://facebook.com/legacy\"}]",
            UpdatedAt = DateTime.UtcNow
        };
        db.ContactInfo.Add(legacyInfo);
        await db.SaveChangesAsync();

        var loaded = await service.GetContactInfoAsync();
        Assert.Equal("legacy@blog.com", loaded.Email);
        Assert.Equal("Đà Nẵng", loaded.Address);
        Assert.Equal("Đà Nẵng", loaded.Location);
        Assert.Null(loaded.Phone);
        Assert.Null(loaded.Bio);
        Assert.Equal("https://github.com/legacy", loaded.GithubUrl);
        Assert.Equal("https://facebook.com/legacy", loaded.FacebookUrl);
        Assert.Equal(2, loaded.SocialLinks.Count);
    }
}
