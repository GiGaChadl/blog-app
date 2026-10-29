using BlogApp.Domain.Enums;

namespace BlogApp.Application.Features.Interactions.Subscribers;

public record SubscribeRequest(string Email);

public record ConfirmSubscriptionRequest(string Token);

public record UnsubscribeRequest(string Email);

public record SubscriberDto(
    Guid Id,
    string Email,
    SubscriberStatus Status,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? UnsubscribedAt
)
{
    public bool IsConfirmed => ConfirmedAt.HasValue || Status == SubscriberStatus.Active;
    public bool IsActive => Status == SubscriberStatus.Active;
}
