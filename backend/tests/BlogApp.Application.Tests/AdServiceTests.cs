using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Features.Administration.Ads;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class AdServiceTests
{
    private (TestApplicationDbContext Db, AdService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var service = new AdService(db);

        return (db, service);
    }

    [Fact]
    public async Task UpsertAdAsync_NewPosition_CreatesAd()
    {
        var (db, service) = CreateService();

        var dto = new UpsertAdDto("ca-pub-12345/home-mid", null, null, true);
        var result = await service.UpsertAdAsync(AdPosition.HomeMid, dto);

        Assert.NotNull(result);
        Assert.Equal(AdPosition.HomeMid, result.Position);
        Assert.Equal("ca-pub-12345/home-mid", result.AdsenseSlotId);

        var count = await db.Ads.CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task UpsertAdAsync_ExistingPosition_UpdatesAd()
    {
        var (db, service) = CreateService();

        db.Ads.Add(new Ad
        {
            Id = Guid.NewGuid(),
            Position = AdPosition.PostDetail,
            AdsenseSlotId = "old-slot",
            IsActive = false
        });
        await db.SaveChangesAsync();

        var dto = new UpsertAdDto("new-slot-id", null, null, true);
        var result = await service.UpsertAdAsync(AdPosition.PostDetail, dto);

        Assert.Equal("new-slot-id", result.AdsenseSlotId);
        Assert.True(result.IsActive);

        // Chỉ có 1 bản ghi duy nhất cho vị trí này
        var count = await db.Ads.CountAsync(a => a.Position == AdPosition.PostDetail);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task UpsertAdAsync_StartDateAfterEndDate_ThrowsValidationException()
    {
        var (_, service) = CreateService();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dto = new UpsertAdDto("slot-id", today.AddDays(10), today.AddDays(5), true);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpsertAdAsync(AdPosition.HomeMid, dto));
    }

    [Fact]
    public async Task GetActiveAdByPositionAsync_ActiveAndWithinDateRange_ReturnsAd()
    {
        var (db, service) = CreateService();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.Ads.Add(new Ad
        {
            Id = Guid.NewGuid(),
            Position = AdPosition.HomeMid,
            AdsenseSlotId = "valid-slot",
            StartDate = today.AddDays(-1),
            EndDate = today.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();

        var activeAd = await service.GetActiveAdByPositionAsync(AdPosition.HomeMid);
        Assert.NotNull(activeAd);
        Assert.Equal("valid-slot", activeAd.AdsenseSlotId);
    }

    [Fact]
    public async Task GetActiveAdByPositionAsync_Expired_ReturnsNull()
    {
        var (db, service) = CreateService();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.Ads.Add(new Ad
        {
            Id = Guid.NewGuid(),
            Position = AdPosition.HomeMid,
            AdsenseSlotId = "expired-slot",
            StartDate = today.AddDays(-10),
            EndDate = today.AddDays(-1), // Đã hết hạn
            IsActive = true
        });
        await db.SaveChangesAsync();

        var activeAd = await service.GetActiveAdByPositionAsync(AdPosition.HomeMid);
        Assert.Null(activeAd);
    }
}
