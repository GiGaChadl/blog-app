using BlogApp.Application.Features.System.Maintenance;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BlogApp.Application.Tests;

public class SystemMaintenanceServiceTests
{
    private (TestApplicationDbContext Db, SystemMaintenanceService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var logger = NullLogger<SystemMaintenanceService>.Instance;
        var service = new SystemMaintenanceService(db, logger);

        return (db, service);
    }

    [Fact]
    public async Task CleanExpiredSessionsAndOtpsAsync_RemovesExpiredData()
    {
        var (db, service) = CreateService();

        var expiredSession = new ReviewSession
        {
            Id = Guid.NewGuid(),
            Email = "expired@domain.com",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(-1) // Hết hạn
        };
        var validSession = new ReviewSession
        {
            Id = Guid.NewGuid(),
            Email = "active@domain.com",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(23)
        };
        db.ReviewSessions.AddRange(expiredSession, validSession);

        var expiredOtp = new OtpVerification
        {
            Id = Guid.NewGuid(),
            Email = "otp@test.com",
            OtpCodeHash = "hash",
            Purpose = OtpPurpose.ReviewVerification,
            IpAddress = "1.2.3.4",
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            ExpiresAt = DateTime.UtcNow.AddDays(-2) // Hết hạn quá 24h
        };
        db.OtpVerifications.Add(expiredOtp);
        await db.SaveChangesAsync();

        var cleanedCount = await service.CleanExpiredSessionsAndOtpsAsync();

        Assert.Equal(2, cleanedCount);
        Assert.False(await db.ReviewSessions.AnyAsync(s => s.Id == expiredSession.Id));
        Assert.True(await db.ReviewSessions.AnyAsync(s => s.Id == validSession.Id));
        Assert.False(await db.OtpVerifications.AnyAsync(o => o.Id == expiredOtp.Id));
    }

    [Fact]
    public async Task ResetDailyEmailCountersAsync_WhenNewDay_ResetsEmailsSentToday()
    {
        var (db, service) = CreateService();

        var settings = new SmtpSettings
        {
            Id = Guid.NewGuid(),
            Host = "smtp.test.com",
            EmailsSentToday = 45,
            LastResetAt = DateTime.UtcNow.AddDays(-1) // Ngày hôm qua
        };
        db.SmtpSettings.Add(settings);
        await db.SaveChangesAsync();

        await service.ResetDailyEmailCountersAsync();

        await db.Entry(settings).ReloadAsync();
        Assert.Equal(0, settings.EmailsSentToday);
        Assert.Equal(DateTime.UtcNow.Date, settings.LastResetAt?.Date);
    }

    [Fact]
    public async Task AggregateMonthlyStatsAndPurgeLogsAsync_AggregatesBeforePurgingLogs()
    {
        var (db, service) = CreateService();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var lastMonthFirstDay = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);

        // Tạo các log nằm trong tháng trước (đồng thời cũ hơn 30 ngày)
        var logDate = lastMonthFirstDay.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc);
        db.VisitorLogs.AddRange(
            new VisitorLog { Id = Guid.NewGuid(), IpAddress = "1.1.1.1", VisitedAt = logDate },
            new VisitorLog { Id = Guid.NewGuid(), IpAddress = "1.1.1.1", VisitedAt = logDate.AddMinutes(5) },
            new VisitorLog { Id = Guid.NewGuid(), IpAddress = "2.2.2.2", VisitedAt = logDate.AddMinutes(10) }
        );

        // Thêm 1 log mới hôm nay (không bị xoá)
        var recentLog = new VisitorLog { Id = Guid.NewGuid(), IpAddress = "3.3.3.3", VisitedAt = now };
        db.VisitorLogs.Add(recentLog);

        await db.SaveChangesAsync();

        // Chạy quy trình bảo trì
        await service.AggregateMonthlyStatsAndPurgeLogsAsync();

        // 1. Kiểm tra thống kê tháng trước ĐÃ ĐƯỢC TỔNG HỢP từ log trước khi bị xoá
        var stats = await db.VisitorStatsMonthly.FirstOrDefaultAsync(s => s.Month == lastMonthFirstDay);
        Assert.NotNull(stats);
        Assert.Equal(3, stats.TotalVisits);
        Assert.Equal(2, stats.UniqueVisitors); // 1.1.1.1 và 2.2.2.2

        // 2. Kiểm tra log cũ hơn 30 ngày đã bị xoá, log mới hôm nay vẫn còn
        var remainingLogs = await db.VisitorLogs.ToListAsync();
        Assert.Single(remainingLogs);
        Assert.Equal(recentLog.Id, remainingLogs[0].Id);
    }
}
