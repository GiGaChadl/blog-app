using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Common.Models;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.Interactions.Reviews;

public class PostReviewService
{
    private readonly IApplicationDbContext _db;

    public PostReviewService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy danh sách đánh giá của bài viết (công khai).
    /// Chỉ hiển thị các đánh giá có trạng thái Visible và email đã được che giấu một phần (MaskedEmail).
    /// </summary>
    public async Task<PagedResult<ReviewDto>> GetPostReviewsAsync(Guid postId, int page = 1, int pageSize = 10, Guid? activeSessionId = null, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        Domain.Entities.ReviewSession? session = null;
        if (activeSessionId.HasValue && activeSessionId.Value != Guid.Empty)
        {
            session = await _db.ReviewSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == activeSessionId.Value && s.ExpiresAt > DateTime.UtcNow, ct);
        }

        var query = _db.PostReviews
            .AsNoTracking()
            .Where(r => r.PostId == postId && r.Status == ReviewStatus.Visible);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.PostId,
                r.Email,
                r.Rating,
                r.Comment,
                r.Status,
                r.CreatedAt
            })
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var dtos = items.Select(r =>
        {
            var canModify = session != null &&
                            string.Equals(r.Email, session.Email, StringComparison.OrdinalIgnoreCase) &&
                            (now - r.CreatedAt).TotalHours <= 24;

            return new ReviewDto(
                r.Id,
                r.PostId,
                MaskEmail(r.Email),
                r.Rating,
                r.Comment,
                r.Status,
                r.CreatedAt,
                canModify
            );
        }).ToList();

        return new PagedResult<ReviewDto>(dtos, total, page, pageSize);
    }

    /// <summary>
    /// Độc giả gửi đánh giá bài viết kèm ReviewSession hợp lệ (còn hạn 24 giờ).
    /// </summary>
    public async Task<ReviewDto> CreateReviewAsync(Guid postId, CreateReviewDto dto, Guid? headerSessionId, CancellationToken ct = default)
    {
        if (dto.Rating < 1 || dto.Rating > 5)
            throw new ValidationException("Điểm đánh giá phải từ 1 đến 5 sao.");

        var sessionId = dto.SessionId ?? headerSessionId;
        if (!sessionId.HasValue || sessionId.Value == Guid.Empty)
            throw new ValidationException("Vui lòng xác thực email qua mã OTP trước khi đánh giá bài viết.");

        var session = await _db.ReviewSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId.Value, ct);

        if (session == null || session.ExpiresAt <= DateTime.UtcNow)
            throw new ValidationException("Phiên đánh giá của bạn đã hết hạn hoặc không tồn tại. Vui lòng lấy mã OTP mới.");

        var post = await _db.Posts
            .FirstOrDefaultAsync(p => p.Id == postId && p.Status == PostStatus.Published, ct);

        if (post == null)
            throw new ValidationException("Bài viết không tồn tại hoặc chưa được xuất bản.");

        var review = new PostReview
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            SessionId = session.Id,
            Email = session.Email,
            Rating = dto.Rating,
            Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
            Status = ReviewStatus.Hidden, // Đánh giá mới tạo cần chờ quản trị viên phê duyệt
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.PostReviews.Add(review);
        await _db.SaveChangesAsync(ct);

        // Cập nhật trường cache AvgRating và RatingCount trên bảng posts
        await RefreshPostRatingStatsAsync(postId, ct);

        return new ReviewDto(
            review.Id,
            review.PostId,
            MaskEmail(review.Email),
            review.Rating,
            review.Comment,
            review.Status,
            review.CreatedAt,
            CanModify: true
        );
    }

    /// <summary>
    /// Độc giả chỉnh sửa đánh giá của chính mình trong phiên 24 giờ.
    /// </summary>
    public async Task<ReviewDto> UpdateReviewAsync(Guid reviewId, UpdateReviewDto dto, Guid? headerSessionId, CancellationToken ct = default)
    {
        if (dto.Rating < 1 || dto.Rating > 5)
            throw new ValidationException("Điểm đánh giá phải từ 1 đến 5 sao.");

        var sessionId = dto.SessionId ?? headerSessionId;
        if (!sessionId.HasValue || sessionId.Value == Guid.Empty)
            throw new ValidationException("Vui lòng cung cấp mã phiên xác thực hợp lệ.");

        var session = await _db.ReviewSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId.Value, ct);

        if (session == null || session.ExpiresAt <= DateTime.UtcNow)
            throw new ValidationException("Phiên làm việc đã hết hạn. Vui lòng xác thực lại mã OTP.");

        var review = await _db.PostReviews
            .FirstOrDefaultAsync(r => r.Id == reviewId, ct);

        if (review == null)
            throw new ValidationException("Đánh giá không tồn tại.");

        if (!string.Equals(review.Email, session.Email, StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("Bạn không có quyền chỉnh sửa đánh giá này.");

        if ((DateTime.UtcNow - review.CreatedAt).TotalHours > 24)
            throw new ValidationException("Chỉ được phép chỉnh sửa đánh giá trong vòng 24 giờ kể từ khi gửi.");

        review.Rating = dto.Rating;
        review.Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim();
        review.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await RefreshPostRatingStatsAsync(review.PostId, ct);

        return new ReviewDto(
            review.Id,
            review.PostId,
            MaskEmail(review.Email),
            review.Rating,
            review.Comment,
            review.Status,
            review.CreatedAt,
            CanModify: true
        );
    }

    /// <summary>
    /// Độc giả tự xóa đánh giá của chính mình trong phiên 24 giờ.
    /// </summary>
    public async Task DeleteReviewByOwnerAsync(Guid reviewId, Guid? sessionId, CancellationToken ct = default)
    {
        if (!sessionId.HasValue || sessionId.Value == Guid.Empty)
            throw new ValidationException("Vui lòng cung cấp mã phiên xác thực hợp lệ.");

        var session = await _db.ReviewSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId.Value, ct);

        if (session == null || session.ExpiresAt <= DateTime.UtcNow)
            throw new ValidationException("Phiên làm việc đã hết hạn. Vui lòng xác thực lại mã OTP.");

        var review = await _db.PostReviews
            .FirstOrDefaultAsync(r => r.Id == reviewId, ct);

        if (review == null)
            throw new ValidationException("Đánh giá không tồn tại.");

        if (!string.Equals(review.Email, session.Email, StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("Bạn không có quyền xóa đánh giá này.");

        if ((DateTime.UtcNow - review.CreatedAt).TotalHours > 24)
            throw new ValidationException("Chỉ được phép xóa đánh giá trong vòng 24 giờ kể từ khi gửi.");

        var postId = review.PostId;
        _db.PostReviews.Remove(review);
        await _db.SaveChangesAsync(ct);

        await RefreshPostRatingStatsAsync(postId, ct);
    }

    // ==========================================
    // Quản trị viên (Admin) Endpoints
    // ==========================================

    /// <summary>
    /// Quản trị viên xem toàn bộ đánh giá (hiển thị email đầy đủ, lọc theo postId và status).
    /// </summary>
    public async Task<PagedResult<AdminReviewDto>> GetAllReviewsAsync(Guid? postId, ReviewStatus? status, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.PostReviews
            .AsNoTracking()
            .Include(r => r.Post)
            .AsQueryable();

        if (postId.HasValue)
            query = query.Where(r => r.PostId == postId.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AdminReviewDto(
                r.Id,
                r.PostId,
                r.Post.Title,
                r.Email,
                r.Rating,
                r.Comment,
                r.Status,
                r.CreatedAt,
                r.UpdatedAt,
                r.SessionId
            ))
            .ToListAsync(ct);

        return new PagedResult<AdminReviewDto>(items, total, page, pageSize);
    }

    /// <summary>
    /// Quản trị viên cập nhật trạng thái đánh giá (Visible, Hidden, Flagged).
    /// </summary>
    public async Task UpdateStatusAsync(Guid reviewId, ReviewStatus newStatus, CancellationToken ct = default)
    {
        var review = await _db.PostReviews.FirstOrDefaultAsync(r => r.Id == reviewId, ct);
        if (review == null)
            throw new ValidationException("Đánh giá không tồn tại.");

        review.Status = newStatus;
        review.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await RefreshPostRatingStatsAsync(review.PostId, ct);
    }

    /// <summary>
    /// Quản trị viên xóa vĩnh viễn một đánh giá vi phạm.
    /// </summary>
    public async Task DeleteReviewAsync(Guid reviewId, CancellationToken ct = default)
    {
        var review = await _db.PostReviews.FirstOrDefaultAsync(r => r.Id == reviewId, ct);
        if (review == null)
            throw new ValidationException("Đánh giá không tồn tại.");

        var postId = review.PostId;
        _db.PostReviews.Remove(review);
        await _db.SaveChangesAsync(ct);

        await RefreshPostRatingStatsAsync(postId, ct);
    }

    /// <summary>
    /// Tính toán và đồng bộ lại trường AvgRating và RatingCount được cache trên bảng posts.
    /// </summary>
    private async Task RefreshPostRatingStatsAsync(Guid postId, CancellationToken ct)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId, ct);
        if (post == null) return;

        var visibleReviews = await _db.PostReviews
            .Where(r => r.PostId == postId && r.Status == ReviewStatus.Visible)
            .Select(r => r.Rating)
            .ToListAsync(ct);

        post.RatingCount = visibleReviews.Count;
        post.AvgRating = visibleReviews.Count > 0
            ? Math.Round((decimal)visibleReviews.Average(), 2)
            : 0;

        await _db.SaveChangesAsync(ct);
    }

    public static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return string.Empty;
        var parts = email.Split('@');
        if (parts.Length != 2) return email;

        var name = parts[0];
        var domain = parts[1];

        if (name.Length <= 2)
            return $"{name}***@{domain}";

        return $"{name[..2]}***@{domain}";
    }

    /// <summary>
    /// Thống kê số lượng đánh giá theo từng trạng thái phục vụ hiển thị Dashboard.
    /// </summary>
    public async Task<ReviewStatsDto> GetReviewStatsAsync(CancellationToken ct = default)
    {
        var pendingCount = await _db.PostReviews.CountAsync(r => r.Status == ReviewStatus.Hidden, ct);
        var visibleCount = await _db.PostReviews.CountAsync(r => r.Status == ReviewStatus.Visible, ct);
        var flaggedCount = await _db.PostReviews.CountAsync(r => r.Status == ReviewStatus.Flagged, ct);
        var totalCount = pendingCount + visibleCount + flaggedCount;

        return new ReviewStatsDto(pendingCount, visibleCount, flaggedCount, totalCount);
    }
}
