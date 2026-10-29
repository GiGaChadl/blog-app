namespace BlogApp.Domain.Entities;

/// <summary>Bảng nối many-to-many. Khoá chính kép (PostId, TagId) — không có Id riêng.</summary>
public class PostTag
{
    public Guid PostId { get; set; }
    public Guid TagId { get; set; }

    public Post Post { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
