using BlogApp.Application.Common.Models;
using BlogApp.Application.Features.Interactions.Reviews;
using BlogApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
public class PostReviewsController : ControllerBase
{
    private readonly PostReviewService _reviewService;

    public PostReviewsController(PostReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Lấy danh sách đánh giá của bài viết (công khai, email được ẩn danh).
    /// </summary>
    [HttpGet("api/posts/{postId:guid}/reviews")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetPostReviews(
        Guid postId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? sessionId = null,
        CancellationToken ct = default)
    {
        var activeSessionId = sessionId ?? GetSessionIdFromHeader();
        var result = await _reviewService.GetPostReviewsAsync(postId, page, pageSize, activeSessionId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Độc giả gửi đánh giá bài viết (yêu cầu session 24h sau khi xác thực OTP).
    /// SessionId có thể gửi qua Header X-Review-Session hoặc trường SessionId trong body.
    /// </summary>
    [HttpPost("api/posts/{postId:guid}/reviews")]
    [AllowAnonymous]
    public async Task<ActionResult<ReviewDto>> CreateReview(
        Guid postId,
        [FromBody] CreateReviewDto dto,
        CancellationToken ct)
    {
        var headerSessionId = GetSessionIdFromHeader();
        var result = await _reviewService.CreateReviewAsync(postId, dto, headerSessionId, ct);
        return CreatedAtAction(nameof(GetPostReviews), new { postId }, result);
    }

    /// <summary>
    /// Độc giả chỉnh sửa đánh giá của chính mình trong phiên 24h.
    /// </summary>
    [HttpPut("api/reviews/{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ReviewDto>> UpdateReview(
        Guid id,
        [FromBody] UpdateReviewDto dto,
        CancellationToken ct)
    {
        var headerSessionId = GetSessionIdFromHeader();
        var result = await _reviewService.UpdateReviewAsync(id, dto, headerSessionId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Độc giả tự xóa đánh giá của chính mình trong phiên 24h.
    /// </summary>
    [HttpDelete("api/reviews/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteReviewByOwner(
        Guid id,
        [FromQuery] Guid? sessionId,
        CancellationToken ct)
    {
        var activeSessionId = sessionId ?? GetSessionIdFromHeader();
        await _reviewService.DeleteReviewByOwnerAsync(id, activeSessionId, ct);
        return NoContent();
    }

    // ==========================================
    // Quản trị viên (Admin) Endpoints
    // ==========================================

    /// <summary>
    /// Thống kê số lượng đánh giá theo từng trạng thái (chờ duyệt, hiển thị, gắn cờ).
    /// </summary>
    [HttpGet("api/admin/reviews/stats")]
    [Authorize]
    public async Task<ActionResult<ReviewStatsDto>> GetReviewStats(CancellationToken ct)
    {
        var result = await _reviewService.GetReviewStatsAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Quản trị viên xem danh sách toàn bộ đánh giá (hiển thị email thật, lọc theo bài viết hoặc trạng thái).
    /// </summary>
    [HttpGet("api/admin/reviews")]
    [Authorize]
    public async Task<ActionResult<PagedResult<AdminReviewDto>>> GetAllReviews(
        [FromQuery] Guid? postId,
        [FromQuery] ReviewStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetAllReviewsAsync(postId, status, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Quản trị viên thay đổi trạng thái kiểm duyệt đánh giá (Visible, Hidden, Flagged).
    /// </summary>
    [HttpPatch("api/admin/reviews/{id:guid}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateReviewStatus(
        Guid id,
        [FromBody] UpdateReviewStatusDto dto,
        CancellationToken ct)
    {
        await _reviewService.UpdateStatusAsync(id, dto.Status, ct);
        return NoContent();
    }

    /// <summary>
    /// Quản trị viên xóa vĩnh viễn một đánh giá.
    /// </summary>
    [HttpDelete("api/admin/reviews/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> AdminDeleteReview(
        Guid id,
        CancellationToken ct)
    {
        await _reviewService.DeleteReviewAsync(id, ct);
        return NoContent();
    }

    private Guid? GetSessionIdFromHeader()
    {
        if (Request.Headers.TryGetValue("X-Review-Session", out var headerValue))
        {
            if (Guid.TryParse(headerValue.FirstOrDefault(), out var parsedGuid))
                return parsedGuid;
        }
        return null;
    }
}
