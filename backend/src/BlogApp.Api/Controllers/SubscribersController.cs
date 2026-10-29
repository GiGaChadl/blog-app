using BlogApp.Application.Common.Models;
using BlogApp.Application.Features.Interactions.Subscribers;
using BlogApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
public class SubscribersController : ControllerBase
{
    private readonly SubscriberService _subscriberService;

    public SubscribersController(SubscriberService subscriberService)
    {
        _subscriberService = subscriberService;
    }

    /// <summary>
    /// Đăng ký nhận bài viết mới (bắt đầu luồng Double Opt-in).
    /// </summary>
    [HttpPost("api/subscribers")]
    [AllowAnonymous]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request, CancellationToken ct)
    {
        var origin = Request.Headers.Origin.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(origin))
        {
            var referer = Request.Headers.Referer.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var uri))
            {
                origin = $"{uri.Scheme}://{uri.Authority}";
            }
        }
        var baseUrl = !string.IsNullOrWhiteSpace(origin) ? origin : $"{Request.Scheme}://{Request.Host}";
        await _subscriberService.SubscribeAsync(request, baseUrl, ct);
        return Ok(new { message = "Vui lòng kiểm tra email của bạn để xác nhận đăng ký nhận bài viết mới." });
    }

    /// <summary>
    /// Xác nhận đăng ký thông qua liên kết trong email.
    /// Trả về trang HTML thông báo kết quả thân thiện cho người dùng.
    /// </summary>
    [HttpGet("api/subscribers/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> Confirm([FromQuery] string token, CancellationToken ct)
    {
        await _subscriberService.ConfirmSubscriptionAsync(token, ct);
        var html = @"
            <!DOCTYPE html>
            <html lang=""vi"">
            <head>
                <meta charset=""UTF-8"">
                <title>Xác nhận đăng ký thành công</title>
                <style>
                    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; display: flex; justify-content: center; align-items: center; min-height: 100vh; margin: 0; background: #f8fafc; color: #1e293b; }
                    .card { background: white; padding: 40px; border-radius: 12px; box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1); max-width: 450px; text-align: center; }
                    h1 { color: #16a34a; font-size: 24px; margin-bottom: 12px; }
                    p { line-height: 1.6; color: #64748b; margin-bottom: 24px; }
                    a { display: inline-block; background: #2563eb; color: white; padding: 10px 20px; border-radius: 6px; text-decoration: none; font-weight: 500; }
                </style>
            </head>
            <body>
                <div class=""card"">
                    <h1>✓ Đăng Ký Thành Công!</h1>
                    <p>Cảm ơn bạn đã xác nhận. Bạn sẽ là người đầu tiên nhận được thông báo khi có bài viết mới nhất trên Blog.</p>
                    <a href=""/"">Về Trang Chủ</a>
                </div>
            </body>
            </html>";
        return Content(html, "text/html");
    }

    /// <summary>
    /// Hủy đăng ký nhận tin.
    /// </summary>
    [HttpPost("api/subscribers/unsubscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request, CancellationToken ct)
    {
        await _subscriberService.UnsubscribeAsync(request, ct);
        return Ok(new { message = "Bạn đã hủy đăng ký nhận bài viết thành công." });
    }

    // ==========================================
    // Quản trị viên (Admin) Endpoints
    // ==========================================

    /// <summary>
    /// Quản trị viên xem danh sách email đăng ký nhận tin (phân trang và lọc theo trạng thái).
    /// </summary>
    [HttpGet("api/admin/subscribers")]
    [Authorize]
    public async Task<ActionResult<PagedResult<SubscriberDto>>> GetSubscribers(
        [FromQuery] SubscriberStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _subscriberService.GetSubscribersAsync(status, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Quản trị viên xóa một email khỏi danh sách người đăng ký.
    /// </summary>
    [HttpDelete("api/admin/subscribers/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteSubscriber(Guid id, CancellationToken ct)
    {
        await _subscriberService.DeleteSubscriberAsync(id, ct);
        return NoContent();
    }
}
