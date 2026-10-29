namespace BlogApp.Application.Features.Administration.Projects;

public record ProjectDto(
    Guid Id,
    Guid MediaId,
    string MediaUrl,
    string? MediaAltText,
    string Title,
    string? ShortDescription,
    string? Tag,
    int SortOrder
);

public record CreateProjectDto(
    Guid MediaId,
    string Title,
    string? ShortDescription,
    string? Tag,
    int SortOrder = 1
);

public record UpdateProjectDto(
    Guid MediaId,
    string Title,
    string? ShortDescription,
    string? Tag,
    int SortOrder = 1
);
