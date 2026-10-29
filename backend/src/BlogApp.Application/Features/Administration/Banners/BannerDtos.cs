using BlogApp.Domain.Enums;

namespace BlogApp.Application.Features.Administration.Banners;

public record BannerDto(
    Guid Id,
    Guid MediaId,
    string MediaUrl,
    string? MediaAltText,
    BannerType Type,
    string? LinkUrl,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAt
)
{
    public string ImageUrl => MediaUrl;
    public string? Title => MediaAltText;
    public string? TargetUrl => LinkUrl;
}

public record CreateBannerDto(
    Guid MediaId,
    BannerType Type = BannerType.Banner,
    string? LinkUrl = null,
    int SortOrder = 1,
    bool IsActive = true,
    string? Title = null,
    string? AltText = null,
    string? TargetUrl = null,
    string? ImageUrl = null
);

public record UpdateBannerDto(
    Guid MediaId,
    BannerType Type = BannerType.Banner,
    string? LinkUrl = null,
    int SortOrder = 1,
    bool IsActive = true,
    string? Title = null,
    string? AltText = null,
    string? TargetUrl = null,
    string? ImageUrl = null
);
