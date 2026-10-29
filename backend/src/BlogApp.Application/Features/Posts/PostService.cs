using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Common.Models;
using BlogApp.Application.Features.Tags;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Application.Features.Posts;

public class PostService
{
    private readonly IApplicationDbContext _db;

    public PostService(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<PostSummaryDto>> GetPagedAsync(PostFilterDto filter, bool isAdmin = false, CancellationToken ct = default)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = filter.PageSize is < 1 or > 100 ? 10 : filter.PageSize;
        IQueryable<Post> query = _db.Posts.AsNoTracking()
            .Include(x => x.Category).Include(x => x.ThumbnailMedia).Include(x => x.PostTags).ThenInclude(x => x.Tag);

        query = !isAdmin ? query.Where(x => x.Status == PostStatus.Published)
            : filter.Status.HasValue ? query.Where(x => x.Status == filter.Status.Value) : query;

        if (filter.CategoryId.HasValue || !string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            var category = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(x =>
                (filter.CategoryId.HasValue && x.Id == filter.CategoryId.Value) ||
                (!string.IsNullOrWhiteSpace(filter.CategorySlug) && x.Slug == filter.CategorySlug), ct);
            if (category is null) return new PagedResult<PostSummaryDto>([], 0, page, pageSize);
            var categoryIds = await GetDescendantCategoryIdsAsync(category.Id, ct);
            query = query.Where(x => categoryIds.Contains(x.CategoryId));
        }

        if (filter.TagId.HasValue || !string.IsNullOrWhiteSpace(filter.TagSlug))
            query = query.Where(x => x.PostTags.Any(pt =>
                (filter.TagId.HasValue && pt.TagId == filter.TagId.Value) ||
                (!string.IsNullOrWhiteSpace(filter.TagSlug) && pt.Tag.Slug == filter.TagSlug)));

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(search) ||
                (x.Excerpt != null && x.Excerpt.ToLower().Contains(search)));
        }

        var total = await query.CountAsync(ct);
        var posts = await query.OrderByDescending(x => x.PublishedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<PostSummaryDto>(posts.Select(ToSummary).ToList(), total, page, pageSize);
    }

    public async Task<PostDetailDto?> GetBySlugAsync(string slug, bool incrementView = false, CancellationToken ct = default)
    {
        var post = await DetailQuery().FirstOrDefaultAsync(x => x.Slug == slug, ct);
        if (post is null) return null;
        if (incrementView)
        {
            post.ViewCount++;
            await _db.SaveChangesAsync(ct);
        }
        return ToDetail(post);
    }

    public async Task<PostDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var post = await DetailQuery().FirstOrDefaultAsync(x => x.Id == id, ct);
        return post is null ? null : ToDetail(post);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
    {
        return await _db.Posts.AsNoTracking().CountAsync(ct);
    }

    public async Task<long> GetTotalViewsAsync(CancellationToken ct = default)
    {
        return await _db.Posts.AsNoTracking().SumAsync(x => (long)x.ViewCount, ct);
    }

    public async Task<List<PostSummaryDto>> GetFeaturedAsync(int count = 5, CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 100);
        var posts = await SummaryQuery().Where(x => x.Status == PostStatus.Published)
            .OrderByDescending(x => x.ViewCount).ThenByDescending(x => x.AvgRating).ThenByDescending(x => x.PublishedAt)
            .Take(count).ToListAsync(ct);
        return posts.Select(ToSummary).ToList();
    }

    public async Task<List<PostSummaryDto>> GetRelatedAsync(Guid postId, int count = 4, CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 100);
        var current = await _db.Posts.AsNoTracking().Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == postId, ct);
        if (current is null) return [];

        var candidates = await SummaryQuery().Where(x => x.Status == PostStatus.Published && x.Id != postId)
            .OrderByDescending(x => x.PublishedAt).ToListAsync(ct);
        var selected = candidates.Where(x => x.CategoryId == current.CategoryId).Take(count).ToList();

        if (selected.Count < count && current.Category.ParentId.HasValue)
        {
            var siblingCategoryIds = await _db.Categories.AsNoTracking()
                .Where(x => x.ParentId == current.Category.ParentId.Value).Select(x => x.Id).ToListAsync(ct);
            selected.AddRange(candidates.Where(x => siblingCategoryIds.Contains(x.CategoryId) && !selected.Any(s => s.Id == x.Id))
                .Take(count - selected.Count));
        }

        if (selected.Count < count)
            selected.AddRange(candidates.Where(x => !selected.Any(s => s.Id == x.Id)).Take(count - selected.Count));
        return selected.Select(ToSummary).ToList();
    }

    public async Task<PostDetailDto> CreateAsync(CreatePostDto dto, CancellationToken ct = default)
    {
        var (title, slug, tagIds) = await ValidateWriteAsync(dto.Title, dto.Slug, dto.CategoryId, dto.ThumbnailMediaId, dto.OgImageMediaId, dto.TagIds, null, ct);
        var now = DateTime.UtcNow;
        var post = new Post
        {
            Id = Guid.NewGuid(), Title = title, Slug = slug, Excerpt = dto.ResolvedExcerpt, ContentHtml = dto.ResolvedContentHtml,
            CategoryId = dto.CategoryId, ThumbnailMediaId = dto.ThumbnailMediaId, OgImageMediaId = dto.OgImageMediaId,
            Status = dto.Status, ReadingTimeMinutes = ResolveReadingTime(dto.ReadingTimeMinutes, dto.ResolvedContentHtml),
            MetaTitle = dto.MetaTitle, MetaDescription = dto.MetaDescription, PublishedAt = now, UpdatedAt = now,
            PostTags = tagIds.Select(id => new PostTag { TagId = id }).ToList()
        };
        _db.Posts.Add(post);
        await SaveWithDuplicateSlugMappingAsync(dto.Slug, null, ct);
        return ToDetail(await DetailQuery().FirstAsync(x => x.Id == post.Id, ct));
    }

    public async Task UpdateAsync(Guid id, UpdatePostDto dto, CancellationToken ct = default)
    {
        var post = await _db.Posts.Include(x => x.PostTags).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Bài viết không tồn tại.");
        var (title, slug, tagIds) = await ValidateWriteAsync(dto.Title, dto.Slug, dto.CategoryId, dto.ThumbnailMediaId, dto.OgImageMediaId, dto.TagIds, id, ct);
        post.Title = title; post.Slug = slug; post.Excerpt = dto.ResolvedExcerpt; post.ContentHtml = dto.ResolvedContentHtml;
        post.CategoryId = dto.CategoryId; post.ThumbnailMediaId = dto.ThumbnailMediaId; post.OgImageMediaId = dto.OgImageMediaId;
        post.Status = dto.Status; post.ReadingTimeMinutes = ResolveReadingTime(dto.ReadingTimeMinutes, dto.ResolvedContentHtml);
        post.MetaTitle = dto.MetaTitle; post.MetaDescription = dto.MetaDescription; post.UpdatedAt = DateTime.UtcNow;
        var removed = post.PostTags.Where(x => !tagIds.Contains(x.TagId)).ToList();
        _db.PostTags.RemoveRange(removed);
        foreach (var tagId in tagIds.Where(tagId => !post.PostTags.Any(x => x.TagId == tagId)))
            post.PostTags.Add(new PostTag { PostId = id, TagId = tagId });
        await SaveWithDuplicateSlugMappingAsync(dto.Slug, id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Bài viết không tồn tại.");
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync(ct);
    }

    private IQueryable<Post> SummaryQuery() => _db.Posts.AsNoTracking().Include(x => x.Category).Include(x => x.ThumbnailMedia).Include(x => x.PostTags).ThenInclude(x => x.Tag);
    private IQueryable<Post> DetailQuery() => _db.Posts.Include(x => x.Category).Include(x => x.ThumbnailMedia).Include(x => x.OgImageMedia).Include(x => x.PostTags).ThenInclude(x => x.Tag);
    private static PostSummaryDto ToSummary(Post x) => new(x.Id, x.Title, x.Slug, x.Excerpt, x.ThumbnailMedia.Url, x.ThumbnailMedia.AltText, x.CategoryId, x.Category.Name, x.Category.Slug, x.PostTags.Select(pt => pt.Tag.Name).ToList(), x.Status, x.ViewCount, x.AvgRating, x.RatingCount, x.ReadingTimeMinutes, x.PublishedAt);
    private static PostDetailDto ToDetail(Post x) => new(x.Id, x.Title, x.Slug, x.Excerpt, x.ContentHtml, x.ThumbnailMedia.Url, x.ThumbnailMedia.AltText, x.OgImageMedia?.Url, x.CategoryId, x.Category.Name, x.Category.Slug, x.PostTags.Select(pt => new TagDto(pt.Tag.Id, pt.Tag.Name, pt.Tag.Slug, pt.Tag.PostTags.Count)).ToList(), x.Status, x.ViewCount, x.AvgRating, x.RatingCount, x.ReadingTimeMinutes, x.MetaTitle, x.MetaDescription, x.PublishedAt, x.UpdatedAt, x.ThumbnailMediaId, x.OgImageMediaId);

    private async Task<(string Title, string Slug, List<Guid> TagIds)> ValidateWriteAsync(string rawTitle, string rawSlug, Guid categoryId, Guid thumbnailId, Guid? ogImageId, List<Guid>? rawTagIds, Guid? postId, CancellationToken ct)
    {
        var title = rawTitle.Trim(); var slug = rawSlug.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(title)) throw new ValidationException("Tiêu đề bài viết không được để trống.");
        if (string.IsNullOrWhiteSpace(slug)) throw new ValidationException("Slug không được để trống.");
        var category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == categoryId, ct);
        if (category is null || category.Level != 3) throw new ValidationException("Bài viết chỉ được phép gắn vào danh mục cấp 3.");
        if (!await _db.MediaLibrary.AnyAsync(x => x.Id == thumbnailId, ct)) throw new ValidationException("Ảnh thumbnail không tồn tại.");
        if (ogImageId.HasValue && !await _db.MediaLibrary.AnyAsync(x => x.Id == ogImageId.Value, ct)) throw new ValidationException("Ảnh OG không tồn tại.");
        if (await _db.Posts.AnyAsync(x => x.Slug == slug && (!postId.HasValue || x.Id != postId.Value), ct)) throw new ValidationException($"Slug \"{rawSlug}\" đã tồn tại.");
        var tagIds = rawTagIds?.Distinct().ToList() ?? [];
        if (tagIds.Count != await _db.Tags.CountAsync(x => tagIds.Contains(x.Id), ct)) throw new ValidationException("Một hoặc nhiều tag không tồn tại.");
        return (title, slug, tagIds);
    }

    private async Task SaveWithDuplicateSlugMappingAsync(string slug, Guid? postId, CancellationToken ct)
    {
        try { await _db.SaveChangesAsync(ct); }
        catch (DbUpdateException)
        {
            if (await _db.Posts.AnyAsync(x => x.Slug == slug && (!postId.HasValue || x.Id != postId.Value), ct)) throw new ValidationException($"Slug \"{slug}\" đã tồn tại.");
            throw;
        }
    }

    private async Task<List<Guid>> GetDescendantCategoryIdsAsync(Guid rootId, CancellationToken ct)
    {
        var categories = await _db.Categories.AsNoTracking().Select(x => new { x.Id, x.ParentId }).ToListAsync(ct);
        var ids = new List<Guid> { rootId };
        for (var i = 0; i < ids.Count; i++) ids.AddRange(categories.Where(x => x.ParentId == ids[i]).Select(x => x.Id).Where(id => !ids.Contains(id)));
        return ids;
    }

    private static int ResolveReadingTime(int? supplied, string? content) => supplied is > 0 ? supplied.GetValueOrDefault() : Math.Max(1, (int)Math.Ceiling((string.IsNullOrWhiteSpace(content) ? 0 : content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length) / 200d));
}
