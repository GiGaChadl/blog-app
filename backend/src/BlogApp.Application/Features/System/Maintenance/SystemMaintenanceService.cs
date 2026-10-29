using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogApp.Application.Features.System.Maintenance;

public class SystemMaintenanceService
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<SystemMaintenanceService> _logger;

    public SystemMaintenanceService(IApplicationDbContext db, ILogger<SystemMaintenanceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Thực thi toàn bộ chu trình bảo trì hệ thống.
    /// </summary>
    public async Task RunMaintenanceAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Bắt đầu chu trình bảo trì hệ thống tự động...");

        await CleanExpiredSessionsAndOtpsAsync(ct);
        await ResetDailyEmailCountersAsync(ct);
        await AggregateMonthlyStatsAndPurgeLogsAsync(ct);

        _logger.LogInformation("Hoàn tất chu trình bảo trì hệ thống.");
    }

    /// <summary>
    /// Dọn dẹp ReviewSessions và OtpVerifications đã hết hạn.
    /// Nhờ ON DELETE SET NULL ở DB, các PostReview thuộc session bị xoá sẽ tự động chuyển SessionId = null
    /// và bài đánh giá của độc giả vẫn được bảo toàn nguyên vẹn.
    /// </summary>
    public async Task<int> CleanExpiredSessionsAndOtpsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // 1. Xoá ReviewSessions hết hạn
        var expiredSessions = await _db.ReviewSessions
            .Where(s => s.ExpiresAt < now)
            .ToListAsync(ct);

        if (expiredSessions.Count > 0)
        {
            _db.ReviewSessions.RemoveRange(expiredSessions);
        }

        // 2. Xoá OTP hết hạn quá 24h
        var expiredCutoff = now.AddHours(-24);
        var oldOtps = await _db.OtpVerifications
            .Where(o => o.ExpiresAt < expiredCutoff)
            .ToListAsync(ct);

        if (oldOtps.Count > 0)
        {
            _db.OtpVerifications.RemoveRange(oldOtps);
        }

        await _db.SaveChangesAsync(ct);
        return expiredSessions.Count + oldOtps.Count;
    }

    /// <summary>
    /// Tự động reset bộ đếm email gửi trong ngày (EmailsSentToday) khi bước sang ngày mới.
    /// </summary>
    public async Task ResetDailyEmailCountersAsync(CancellationToken ct = default)
    {
        var settings = await _db.SmtpSettings.FirstOrDefaultAsync(ct);
        if (settings == null) return;

        var now = DateTime.UtcNow;
        if (!settings.LastResetAt.HasValue || settings.LastResetAt.Value.Date < now.Date)
        {
            settings.EmailsSentToday = 0;
            settings.LastResetAt = now;
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Đã reset bộ đếm email gửi trong ngày của SMTP.");
        }
    }

    /// <summary>
    /// Tổng hợp thống kê tháng và dọn dẹp log quá 30 ngày.
    /// THỨ TỰ BẮT BUỘC (PROJECT-MASTER-GUIDE §10):
    /// PHẢI tổng hợp số liệu của tháng trước vào VisitorStatsMonthly TRƯỚC KHI xoá VisitorLogs cũ hơn 30 ngày.
    /// </summary>
    public async Task AggregateMonthlyStatsAndPurgeLogsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Tháng trước (ngày 01 của tháng trước)
        var lastMonthFirstDay = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);
        var currentMonthFirstDay = new DateOnly(today.Year, today.Month, 1);

        // 1. Kiểm tra tháng trước đã được tổng hợp chưa
        var alreadyAggregated = await _db.VisitorStatsMonthly
            .AnyAsync(s => s.Month == lastMonthFirstDay, ct);

        if (!alreadyAggregated)
        {
            var startUtc = lastMonthFirstDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endUtc = currentMonthFirstDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            var monthLogs = await _db.VisitorLogs
                .Where(l => l.VisitedAt >= startUtc && l.VisitedAt < endUtc)
                .Select(l => l.IpAddress)
                .ToListAsync(ct);

            if (monthLogs.Count > 0)
            {
                var totalVisits = monthLogs.Count;
                var uniqueVisitors = monthLogs.Where(ip => !string.IsNullOrWhiteSpace(ip)).Distinct().Count();

                var stats = new VisitorStatsMonthly
                {
                    Id = Guid.NewGuid(),
                    Month = lastMonthFirstDay,
                    TotalVisits = totalVisits,
                    UniqueVisitors = uniqueVisitors > 0 ? uniqueVisitors : 1,
                    CreatedAt = now
                };

                _db.VisitorStatsMonthly.Add(stats);
                await _db.SaveChangesAsync(ct);
                _logger.LogInformation("Đã tổng hợp thống kê tháng {Month}: {Visits} visits, {Uniques} uniques.", lastMonthFirstDay, totalVisits, uniqueVisitors);
            }
        }

        // 2. Xoá VisitorLogs cũ hơn 30 ngày (Chạy SAU KHI đã tổng hợp)
        var purgeCutoff = now.AddDays(-30);
        var oldLogs = await _db.VisitorLogs
            .Where(l => l.VisitedAt < purgeCutoff)
            .ToListAsync(ct);

        if (oldLogs.Count > 0)
        {
            _db.VisitorLogs.RemoveRange(oldLogs);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Đã dọn dẹp {Count} bản ghi visitor logs cũ hơn 30 ngày.", oldLogs.Count);
        }
    }
}
