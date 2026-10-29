using BlogApp.Domain.Enums;

namespace BlogApp.Domain.Entities;

public class Post
{
    public Guid Id { get; set; }

    /// <summary>
    /// BẮT BUỘC trỏ vào danh mục CẤP 3 — ép bằng trigger DB (fn_check_post_category_level).
    /// Tầng Application NÊN tự validate trước khi gọi Insert/Update để trả lỗi thân thiện,
    /// không nên chỉ dựa vào exception thô từ trigger (xem ghi chú "2 tầng bảo vệ" đã bàn).
    /// </summary>
    public Guid CategoryId { get; set; }

    public Guid ThumbnailMediaId { get; set; }
    public Guid? OgImageMediaId { get; set; }

    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Excerpt { get; set; }
    public string? ContentHtml { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Published;
    public int ViewCount { get; set; }

    /// <summary>Cache — cập nhật lại mỗi khi post_reviews thay đổi, không tính lại mỗi lần đọc.</summary>
    public decimal AvgRating { get; set; }
    public int RatingCount { get; set; }

    public int? ReadingTimeMinutes { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    /// <summary>
    /// Không có Draft — lưu là public ngay. published_at = created_at (không tách 2 mốc thời gian).
    /// </summary>
    public DateTime PublishedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Category Category { get; set; } = null!;
    public MediaLibrary ThumbnailMedia { get; set; } = null!;
    public MediaLibrary? OgImageMedia { get; set; }
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public ICollection<PostReview> Reviews { get; set; } = new List<PostReview>();
}
