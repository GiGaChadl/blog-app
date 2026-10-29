using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Features.Administration.Banners;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class BannerServiceTests
{
    private (TestApplicationDbContext Db, BannerService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var service = new BannerService(db);

        return (db, service);
    }

    [Fact]
    public async Task CreateAsync_InvalidMediaId_ThrowsValidationException()
    {
        var (_, service) = CreateService();

        var dto = new CreateBannerDto(Guid.NewGuid(), BannerType.Banner, "https://link.com");
        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(dto));
        Assert.Contains("không tồn tại", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ValidMediaId_CreatesBanner()
    {
        var (db, service) = CreateService();

        var media = new MediaLibrary
        {
            Id = Guid.NewGuid(),
            FileName = "banner.png",
            Url = "/uploads/banner.png",
            UsageType = MediaUsageType.HeroBanner,
            UploadedAt = DateTime.UtcNow
        };
        db.MediaLibrary.Add(media);
        await db.SaveChangesAsync();

        var dto = new CreateBannerDto(media.Id, BannerType.Banner, "https://google.com", 1, true);
        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(media.Id, result.MediaId);
        Assert.Equal(BannerType.Banner, result.Type);
        Assert.Equal(1, result.SortOrder);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetActiveBannersAsync_FiltersOnlyActiveAndSorted()
    {
        var (db, service) = CreateService();

        var media1 = new MediaLibrary { Id = Guid.NewGuid(), FileName = "m1.png", Url = "/m1.png", UploadedAt = DateTime.UtcNow };
        var media2 = new MediaLibrary { Id = Guid.NewGuid(), FileName = "m2.png", Url = "/m2.png", UploadedAt = DateTime.UtcNow };
        var media3 = new MediaLibrary { Id = Guid.NewGuid(), FileName = "m3.png", Url = "/m3.png", UploadedAt = DateTime.UtcNow };
        db.MediaLibrary.AddRange(media1, media2, media3);

        db.Banners.AddRange(
            new Banner { Id = Guid.NewGuid(), MediaId = media1.Id, Type = BannerType.Banner, SortOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Banner { Id = Guid.NewGuid(), MediaId = media2.Id, Type = BannerType.Banner, SortOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Banner { Id = Guid.NewGuid(), MediaId = media3.Id, Type = BannerType.Banner, SortOrder = 3, IsActive = false, CreatedAt = DateTime.UtcNow } // Inactive
        );
        await db.SaveChangesAsync();

        var list = await service.GetActiveBannersAsync(BannerType.Banner);

        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0].SortOrder);
        Assert.Equal(2, list[1].SortOrder);
    }

    [Fact]
    public async Task CreateAsync_WhenSortOrderLessThanOne_ThrowsValidationException()
    {
        var (db, service) = CreateService();
        var media = new MediaLibrary { Id = Guid.NewGuid(), FileName = "b.png", Url = "/b.png", UploadedAt = DateTime.UtcNow };
        db.MediaLibrary.Add(media);
        await db.SaveChangesAsync();

        var dto = new CreateBannerDto(media.Id, BannerType.Banner, null, 0);
        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(dto));
        Assert.Contains("Thứ tự sắp xếp phải lớn hơn hoặc bằng 1", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_ExistingBanner_RemovesSuccessfully()
    {
        var (db, service) = CreateService();

        var media = new MediaLibrary { Id = Guid.NewGuid(), FileName = "del.png", Url = "/del.png", UploadedAt = DateTime.UtcNow };
        db.MediaLibrary.Add(media);

        var banner = new Banner { Id = Guid.NewGuid(), MediaId = media.Id, Type = BannerType.Slider, CreatedAt = DateTime.UtcNow };
        db.Banners.Add(banner);
        await db.SaveChangesAsync();

        await service.DeleteAsync(banner.Id);

        var exists = await db.Banners.AnyAsync(b => b.Id == banner.Id);
        Assert.False(exists);
    }
}
