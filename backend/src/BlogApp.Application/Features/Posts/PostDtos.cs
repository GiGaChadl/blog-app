using BlogApp.Application.Features.Tags;
using BlogApp.Domain.Enums;

namespace BlogApp.Application.Features.Posts;

public record PostSummaryDto(Guid Id, string Title, string Slug, string? Excerpt, string ThumbnailUrl, string? ThumbnailAlt, Guid CategoryId, string CategoryName, string CategorySlug, List<string> Tags, PostStatus Status, int ViewCount, decimal AvgRating, int RatingCount, int? ReadingTimeMinutes, DateTime PublishedAt);

public record PostDetailDto(Guid Id, string Title, string Slug, string? Excerpt, string? ContentHtml, string ThumbnailUrl, string? ThumbnailAlt, string? OgImageUrl, Guid CategoryId, string CategoryName, string CategorySlug, List<TagDto> Tags, PostStatus Status, int ViewCount, decimal AvgRating, int RatingCount, int? ReadingTimeMinutes, string? MetaTitle, string? MetaDescription, DateTime PublishedAt, DateTime UpdatedAt, Guid ThumbnailMediaId = default, Guid? OgImageMediaId = null);

public record PostFilterDto(Guid? CategoryId = null, string? CategorySlug = null, Guid? TagId = null, string? TagSlug = null, string? Search = null, PostStatus? Status = null, int Page = 1, int PageSize = 10);

public record CreatePostDto(
    string Title,
    string Slug,
    string? Excerpt = null,
    string? ContentHtml = null,
    Guid CategoryId = default,
    Guid ThumbnailMediaId = default,
    Guid? OgImageMediaId = null,
    List<Guid>? TagIds = null,
    PostStatus Status = PostStatus.Published,
    int? ReadingTimeMinutes = null,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string? Summary = null,
    string? Content = null
)
{
    public string? ResolvedExcerpt => !string.IsNullOrWhiteSpace(Excerpt) ? Excerpt : Summary;
    public string? ResolvedContentHtml => !string.IsNullOrWhiteSpace(ContentHtml) ? ContentHtml : Content;
}

public record UpdatePostDto(
    string Title,
    string Slug,
    string? Excerpt = null,
    string? ContentHtml = null,
    Guid CategoryId = default,
    Guid ThumbnailMediaId = default,
    Guid? OgImageMediaId = null,
    List<Guid>? TagIds = null,
    PostStatus Status = PostStatus.Published,
    int? ReadingTimeMinutes = null,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string? Summary = null,
    string? Content = null
)
{
    public string? ResolvedExcerpt => !string.IsNullOrWhiteSpace(Excerpt) ? Excerpt : Summary;
    public string? ResolvedContentHtml => !string.IsNullOrWhiteSpace(ContentHtml) ? ContentHtml : Content;
}
