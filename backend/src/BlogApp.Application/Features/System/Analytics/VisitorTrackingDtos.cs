namespace BlogApp.Application.Features.System.Analytics;

public record RecordVisitRequest(
    string VisitorToken,
    string? PageUrl,
    string? Referrer,
    string? DeviceType
);

public record MonthlyStatsDto(
    DateOnly Month,
    int TotalVisits,
    int UniqueVisitors,
    DateTime CreatedAt
);

public record TrackingResultDto(
    bool Recorded,
    string? Message
);
