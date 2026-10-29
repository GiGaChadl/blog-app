using BlogApp.Domain.Enums;

namespace BlogApp.Domain.Entities;

public class Banner
{
    public Guid Id { get; set; }
    public Guid MediaId { get; set; }
    public BannerType Type { get; set; }
    public string? LinkUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public MediaLibrary Media { get; set; } = null!;
}
