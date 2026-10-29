using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Features.Media;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BlogApp.Application.Tests;

public class MediaServiceTests
{
    [Theory]
    [InlineData("test.exe")]
    [InlineData("script.sh")]
    [InlineData("hack.php")]
    [InlineData("document.pdf")]
    public async Task UploadAsync_WhenInvalidExtension_ThrowsValidationException(string fileName)
    {
        await using var db = CreateContext();
        var storage = new TestFileStorageService();
        var service = new MediaService(db, storage);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake content"));
        var request = new UploadMediaRequest(stream, fileName, "application/octet-stream", stream.Length, "Alt", MediaUsageType.PostContent);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.UploadAsync(request));
        Assert.Contains("không được hỗ trợ", ex.Message);
    }

    [Fact]
    public async Task UploadAsync_WhenFileSizeExceedsLimit_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var storage = new TestFileStorageService();
        var service = new MediaService(db, storage);

        using var stream = new MemoryStream(new byte[10]);
        var request = new UploadMediaRequest(stream, "large.png", "image/png", 11 * 1024 * 1024, "Alt", MediaUsageType.PostThumbnail);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.UploadAsync(request));
        Assert.Contains("vượt quá giới hạn", ex.Message);
    }

    [Fact]
    public async Task UploadAsync_WhenValidPng_SavesFileAndReturnsMediaDto()
    {
        await using var db = CreateContext();
        var storage = new TestFileStorageService();
        var service = new MediaService(db, storage);

        // Header PNG chuẩn: 8 bytes magic + 4 bytes chunk length + 4 bytes 'IHDR' + 4 bytes width (100) + 4 bytes height (200)
        byte[] pngHeader = new byte[24];
        pngHeader[0] = 0x89; pngHeader[1] = 0x50; pngHeader[2] = 0x4E; pngHeader[3] = 0x47;
        pngHeader[4] = 0x0D; pngHeader[5] = 0x0A; pngHeader[6] = 0x1A; pngHeader[7] = 0x0A;
        // width = 100 -> 0x00, 0x00, 0x00, 0x64
        pngHeader[16] = 0x00; pngHeader[17] = 0x00; pngHeader[18] = 0x00; pngHeader[19] = 0x64;
        // height = 200 -> 0x00, 0x00, 0x00, 0xC8
        pngHeader[20] = 0x00; pngHeader[21] = 0x00; pngHeader[22] = 0x00; pngHeader[23] = 0xC8;

        using var stream = new MemoryStream(pngHeader);
        var request = new UploadMediaRequest(stream, "photo.png", "image/png", pngHeader.Length, "My Photo", MediaUsageType.PostThumbnail);

        var result = await service.UploadAsync(request);

        Assert.NotNull(result);
        Assert.Equal("photo.png", result.FileName);
        Assert.Equal("My Photo", result.AltText);
        Assert.Equal(MediaUsageType.PostThumbnail, result.UsageType);
        Assert.Equal(100, result.Width);
        Assert.Equal(200, result.Height);
        Assert.Single(storage.SavedFiles);

        var inDb = await db.MediaLibrary.FindAsync(result.Id);
        Assert.NotNull(inDb);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAltTextAndUsageType()
    {
        await using var db = CreateContext();
        var storage = new TestFileStorageService();
        var service = new MediaService(db, storage);

        var media = new MediaLibrary
        {
            Id = Guid.NewGuid(),
            FileName = "banner.webp",
            Url = "/uploads/banner.webp",
            Width = 1200,
            Height = 630,
            UsageType = MediaUsageType.HeroBanner,
            UploadedAt = DateTime.UtcNow
        };
        db.MediaLibrary.Add(media);
        await db.SaveChangesAsync();

        await service.UpdateAsync(media.Id, new UpdateMediaDto("Updated Alt Text", MediaUsageType.OgImage));

        var updated = await db.MediaLibrary.FindAsync(media.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Alt Text", updated.AltText);
        Assert.Equal(MediaUsageType.OgImage, updated.UsageType);
    }

    [Fact]
    public async Task DeleteAsync_WhenUsedByPostThumbnail_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var storage = new TestFileStorageService();
        var service = new MediaService(db, storage);

        var media = new MediaLibrary { Id = Guid.NewGuid(), FileName = "thumb.jpg", Url = "/uploads/thumb.jpg", Width = 100, Height = 100 };
        var category = new Category { Id = Guid.NewGuid(), Name = "Cat", Slug = "cat", Level = 3 };
        var post = new Post { Id = Guid.NewGuid(), Title = "Post", Slug = "post", CategoryId = category.Id, ThumbnailMediaId = media.Id };
        db.AddRange(media, category, post);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.DeleteAsync(media.Id));
        Assert.Contains("thumbnail", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_WhenUsedByBanner_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var storage = new TestFileStorageService();
        var service = new MediaService(db, storage);

        var media = new MediaLibrary { Id = Guid.NewGuid(), FileName = "banner.jpg", Url = "/uploads/banner.jpg", Width = 800, Height = 400 };
        var banner = new Banner { Id = Guid.NewGuid(), MediaId = media.Id, Type = BannerType.Banner };
        db.AddRange(media, banner);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.DeleteAsync(media.Id));
        Assert.Contains("banner", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_WhenUsedByProject_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var storage = new TestFileStorageService();
        var service = new MediaService(db, storage);

        var media = new MediaLibrary { Id = Guid.NewGuid(), FileName = "project.jpg", Url = "/uploads/project.jpg", Width = 600, Height = 400 };
        var project = new Project { Id = Guid.NewGuid(), Title = "Blog Project", MediaId = media.Id };
        db.AddRange(media, project);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.DeleteAsync(media.Id));
        Assert.Contains("dự án", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotUsed_DeletesFileAndRemovesEntity()
    {
        await using var db = CreateContext();
        var storage = new TestFileStorageService();
        var service = new MediaService(db, storage);

        var media = new MediaLibrary { Id = Guid.NewGuid(), FileName = "free.jpg", Url = "/uploads/free.jpg", Width = 300, Height = 300 };
        db.MediaLibrary.Add(media);
        await db.SaveChangesAsync();

        await service.DeleteAsync(media.Id);

        Assert.Null(await db.MediaLibrary.FindAsync(media.Id));
        Assert.Contains(media.Url, storage.DeletedFiles);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByUsageType()
    {
        await using var db = CreateContext();
        var storage = new TestFileStorageService();
        var service = new MediaService(db, storage);

        db.MediaLibrary.AddRange(
            new MediaLibrary { Id = Guid.NewGuid(), FileName = "thumb.jpg", Url = "/uploads/thumb.jpg", UsageType = MediaUsageType.PostThumbnail, Width = 100, Height = 100, UploadedAt = DateTime.UtcNow },
            new MediaLibrary { Id = Guid.NewGuid(), FileName = "banner.jpg", Url = "/uploads/banner.jpg", UsageType = MediaUsageType.HeroBanner, Width = 1200, Height = 400, UploadedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var result = await service.GetPagedAsync(new MediaFilterDto(UsageType: MediaUsageType.HeroBanner));

        Assert.Equal(1, result.Total);
        Assert.Single(result.Items);
        Assert.Equal("banner.jpg", result.Items[0].FileName);
    }

    private static TestApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestApplicationDbContext(options);
    }

    private class TestFileStorageService : IFileStorageService
    {
        public List<string> SavedFiles { get; } = new();
        public List<string> DeletedFiles { get; } = new();

        public Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
        {
            var url = $"/uploads/test-{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
            SavedFiles.Add(url);
            return Task.FromResult(url);
        }

        public Task DeleteFileAsync(string fileUrl, CancellationToken ct = default)
        {
            DeletedFiles.Add(fileUrl);
            return Task.CompletedTask;
        }
    }
}
