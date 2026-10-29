using BlogApp.Domain.Enums;

namespace BlogApp.Application.Features.Interactions.Reviews;

public record ReviewDto(
    Guid Id,
    Guid PostId,
    string MaskedEmail,
    int Rating,
    string? Comment,
    ReviewStatus Status,
    DateTime CreatedAt,
    bool CanModify = false
);

public record AdminReviewDto(
    Guid Id,
    Guid PostId,
    string PostTitle,
    string Email,
    int Rating,
    string? Comment,
    ReviewStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid? SessionId
);

public record CreateReviewDto(
    int Rating,
    string? Comment,
    Guid? SessionId
);

public record UpdateReviewDto(
    int Rating,
    string? Comment,
    Guid? SessionId
);

public record UpdateReviewStatusDto(
    ReviewStatus Status
);

public record ReviewStatsDto(
    int PendingCount,
    int VisibleCount,
    int FlaggedCount,
    int TotalCount
);

