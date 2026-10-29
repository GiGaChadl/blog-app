using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Features.System.CookieConsent;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.System.Analytics;

public class VisitorTrackingService
{
    private readonly IApplicationDbContext _db;
    private readonly CookieConsentService _cookieConsentService;

    public VisitorTrackingService(IApplicationDbContext db, CookieConsentService cookieConsentService)
    {
        _db = db;
        _cookieConsentService = cookieConsentService;
    }

    /// <summary>
    /// Ghi nhận lượt truy cập của độc giả.
    /// BẮT BUỘC kiểm tra độc giả đã bấm Đồng ý Cookie (CookieChoice.Accepted).
    /// Nếu chưa đồng ý, không ghi bất kỳ thông tin nào để bảo đảm quyền riêng tư (PROJECT-MASTER-GUIDE §15).
    /// </summary>
    public async Task<TrackingResultDto> RecordVisitAsync(
        RecordVisitRequest request,
        string? clientIp,
        string? userAgent,
        CancellationToken ct = default)
    {
        var hasAccepted = await _cookieConsentService.HasAcceptedAsync(request.VisitorToken, ct);
        if (!hasAccepted)
        {
            return new TrackingResultDto(false, "Không ghi nhận dữ liệu vì độc giả chưa đồng ý chia sẻ cookie.");
        }

        var log = new VisitorLog
        {
            Id = Guid.NewGuid(),
            IpAddress = string.IsNullOrWhiteSpace(clientIp) ? null : clientIp.Trim(),
            UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim(),
            DeviceType = string.IsNullOrWhiteSpace(request.DeviceType) ? null : request.DeviceType.Trim(),
            PageUrl = string.IsNullOrWhiteSpace(request.PageUrl) ? null : request.PageUrl.Trim(),
            Referrer = string.IsNullOrWhiteSpace(request.Referrer) ? null : request.Referrer.Trim(),
            VisitedAt = DateTime.UtcNow
        };

        _db.VisitorLogs.Add(log);
        await _db.SaveChangesAsync(ct);

        return new TrackingResultDto(true, "Đã ghi nhận lượt truy cập thành công.");
    }

    /// <summary>
    /// Quản trị viên xem báo cáo thống kê theo tháng đã được tổng hợp.
    /// </summary>
    public async Task<List<MonthlyStatsDto>> GetMonthlyStatsAsync(int limit = 12, CancellationToken ct = default)
    {
        return await _db.VisitorStatsMonthly
            .AsNoTracking()
            .OrderByDescending(s => s.Month)
            .Take(limit)
            .Select(s => new MonthlyStatsDto(s.Month, s.TotalVisits, s.UniqueVisitors, s.CreatedAt))
            .ToListAsync(ct);
    }
}
