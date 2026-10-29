namespace BlogApp.Domain.Entities;

/// <summary>Bảng singleton — quy ước chỉ 1 dòng tồn tại, ràng buộc ở tầng Application.</summary>
public class ContactInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string? Location { get; set; }

    /// <summary>
    /// Lưu jsonb dạng chuỗi JSON thô (không dùng auto-mapping POCO của Npgsql để tránh rủi ro
    /// hành vi không kiểm chứng được khi không thể build/test trực tiếp). Serialize/deserialize
    /// thủ công bằng System.Text.Json ở tầng Application khi cần đọc/ghi danh sách SocialLink.
    /// </summary>
    public string? SocialLinksJson { get; set; }

    public DateTime UpdatedAt { get; set; }
}
