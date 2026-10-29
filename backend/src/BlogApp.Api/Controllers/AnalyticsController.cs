using BlogApp.Application.Features.System.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
public class AnalyticsController : ControllerBase
{
    private readonly VisitorTrackingService _trackingService;

    public AnalyticsController(VisitorTrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    /// <summary>Ghi nhận lượt truy cập trang web (chỉ lưu khi đã có sự đồng ý cookie của độc giả).</summary>
    [HttpPost("api/analytics/track")]
    [AllowAnonymous]
    public async Task<ActionResult<TrackingResultDto>> TrackVisit([FromBody] RecordVisitRequest request, CancellationToken ct)
    {
        var clientIp = GetClientIpAddress();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _trackingService.RecordVisitAsync(request, clientIp, userAgent, ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên xem báo cáo thống kê truy cập theo tháng.</summary>
    [HttpGet("api/admin/analytics/monthly")]
    [Authorize]
    public async Task<ActionResult<List<MonthlyStatsDto>>> GetMonthlyStats([FromQuery] int limit = 12, CancellationToken ct = default)
    {
        var result = await _trackingService.GetMonthlyStatsAsync(limit, ct);
        return Ok(result);
    }

    private string? GetClientIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrWhiteSpace(ip))
                return ip;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
