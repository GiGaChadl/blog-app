namespace BlogApp.Application.Features.Administration.Slogans;

public record SloganDto(
    Guid Id,
    string Content,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateSloganDto(
    string Content,
    bool IsActive = false
);

public record UpdateSloganDto(
    string Content,
    bool IsActive
);
