using BlogApp.Domain.Enums;

namespace BlogApp.Domain.Entities;

/// <summary>Double opt-in: Pending -> Active (bấm link, hạn 7 ngày) -> Unsubscribed.</summary>
public class Subscriber
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public SubscriberStatus Status { get; set; } = SubscriberStatus.Pending;
    public string? ConfirmToken { get; set; }
    public DateTime? ConfirmTokenExpiresAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? UnsubscribedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
