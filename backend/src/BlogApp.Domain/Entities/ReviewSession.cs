namespace BlogApp.Domain.Entities;

/// <summary>Sống 24h sau khi xác thực OTP. Cron job xoá vật lý dòng hết hạn mỗi đêm.</summary>
public class ReviewSession
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public ICollection<PostReview> Reviews { get; set; } = new List<PostReview>();
}
