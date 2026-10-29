using BlogApp.Domain.Enums;

namespace BlogApp.Domain.Entities;

public class PostReview
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }

    /// <summary>Null nếu session gốc đã bị cron dọn (ON DELETE SET NULL) — review vẫn giữ nguyên.</summary>
    public Guid? SessionId { get; set; }

    /// <summary>KHÔNG hiển thị công khai — chỉ dùng nội bộ để xác thực chủ sở hữu khi hết session.</summary>
    public string Email { get; set; } = null!;

    public int Rating { get; set; } // 1-5, ép bằng CHECK constraint ở DB
    public string? Comment { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Visible;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Post Post { get; set; } = null!;
    public ReviewSession? Session { get; set; }
}
