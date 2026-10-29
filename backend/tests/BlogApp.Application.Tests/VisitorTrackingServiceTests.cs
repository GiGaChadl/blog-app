using BlogApp.Application.Features.System.Analytics;
using BlogApp.Application.Features.System.CookieConsent;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class VisitorTrackingServiceTests
{
    private (TestApplicationDbContext Db, CookieConsentService CookieService, VisitorTrackingService TrackingService) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var cookieService = new CookieConsentService(db);
        var trackingService = new VisitorTrackingService(db, cookieService);

        return (db, cookieService, trackingService);
    }

    [Fact]
    public async Task RecordVisitAsync_WithoutCookieConsent_DoesNotLogVisit()
    {
        var (db, _, trackingService) = CreateService();

        var request = new RecordVisitRequest("no-consent-token", "/posts/test", "https://google.com", "Desktop");
        var result = await trackingService.RecordVisitAsync(request, "1.2.3.4", "Mozilla/5.0");

        Assert.False(result.Recorded);
        Assert.Contains("chưa đồng ý", result.Message);

        var logCount = await db.VisitorLogs.CountAsync();
        Assert.Equal(0, logCount);
    }

    [Fact]
    public async Task RecordVisitAsync_WithAcceptedCookieConsent_LogsVisitSuccessfully()
    {
        var (db, cookieService, trackingService) = CreateService();

        // Độc giả đồng ý cookie trước
        await cookieService.RecordConsentAsync(new RecordConsentRequest("consented-token", CookieChoice.Accepted));

        var request = new RecordVisitRequest("consented-token", "/posts/kientruc", "https://facebook.com", "Mobile");
        var result = await trackingService.RecordVisitAsync(request, "113.160.0.1", "Mozilla/iPhone");

        Assert.True(result.Recorded);

        var log = await db.VisitorLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal("113.160.0.1", log.IpAddress);
        Assert.Equal("/posts/kientruc", log.PageUrl);
        Assert.Equal("Mobile", log.DeviceType);
    }

    [Fact]
    public async Task GetMonthlyStatsAsync_ReturnsOrderedMonthlyStats()
    {
        var (db, _, trackingService) = CreateService();

        db.VisitorStatsMonthly.AddRange(
            new VisitorStatsMonthly { Id = Guid.NewGuid(), Month = new DateOnly(2026, 7, 1), TotalVisits = 1500, UniqueVisitors = 800, CreatedAt = DateTime.UtcNow },
            new VisitorStatsMonthly { Id = Guid.NewGuid(), Month = new DateOnly(2026, 8, 1), TotalVisits = 2000, UniqueVisitors = 1100, CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var stats = await trackingService.GetMonthlyStatsAsync(12);

        Assert.Equal(2, stats.Count);
        Assert.Equal(new DateOnly(2026, 8, 1), stats[0].Month);
        Assert.Equal(2000, stats[0].TotalVisits);
    }
}
