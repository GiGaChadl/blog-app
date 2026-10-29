using BlogApp.Domain.Enums;

namespace BlogApp.Domain.Entities;

public class MediaLibrary
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int Width { get; set; }
    public int Height { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? AltText { get; set; }
    public MediaUsageType UsageType { get; set; }
    public DateTime UploadedAt { get; set; }

    // Navigation ngược (1 ảnh có thể được nhiều bài viết/banner/dự án dùng làm tham chiếu)
    public ICollection<Post> ThumbnailForPosts { get; set; } = new List<Post>();
    public ICollection<Post> OgImageForPosts { get; set; } = new List<Post>();
    public ICollection<Banner> Banners { get; set; } = new List<Banner>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
