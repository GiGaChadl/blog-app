namespace BlogApp.Domain.Entities;

/// <summary>Tối đa 1 dòng IsActive=true tại 1 thời điểm — ép bằng partial unique index ở DB.</summary>
public class Slogan
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
