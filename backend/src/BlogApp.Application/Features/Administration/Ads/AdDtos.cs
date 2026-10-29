using BlogApp.Domain.Enums;

namespace BlogApp.Application.Features.Administration.Ads;

public record AdDto(
    Guid Id,
    AdPosition Position,
    string AdsenseSlotId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsActive
);

public record UpsertAdDto(
    string AdsenseSlotId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsActive = true
);
