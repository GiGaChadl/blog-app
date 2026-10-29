using BlogApp.Domain.Enums;

namespace BlogApp.Application.Features.Media;

public record MediaDto(
    Guid Id,
    string FileName,
    string Url,
    int Width,
    int Height,
    long? FileSizeBytes,
    string? AltText,
    MediaUsageType UsageType,
    DateTime UploadedAt
);

public record UploadMediaRequest(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string? AltText,
    MediaUsageType UsageType
);

public record UpdateMediaDto(
    string? AltText,
    MediaUsageType UsageType
);

public record MediaFilterDto(
    MediaUsageType? UsageType = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20
);
