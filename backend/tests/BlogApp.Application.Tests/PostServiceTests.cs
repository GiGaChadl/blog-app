using BlogApp.Application.Features.Posts;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Application.Tests;

public class PostServiceTests
{
    [Theory]
    [InlineData((short)1)]
    [InlineData((short)2)]
    public async Task CreateAsync_WhenCategoryNotLevel3_ThrowsValidationException(short level)
    {
        await using var db = CreateContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Category", Slug = "category", Level = level };
        var thumbnail = AddMedia();
        db.AddRange(category, thumbnail);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ValidationException>(() => new PostService(db).CreateAsync(CreateDto(category.Id, thumbnail.Id)));
    }

    [Fact]
    public async Task CreateAsync_WhenThumbnailNotFound_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var category = AddLeafCategory(); db.Categories.Add(category); await db.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<ValidationException>(() => new PostService(db).CreateAsync(CreateDto(category.Id, Guid.NewGuid())));

        Assert.Equal("Ảnh thumbnail không tồn tại.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenSlugAlreadyExists_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var category = AddLeafCategory(); var media = AddMedia();
        db.AddRange(category, media, CreatePost(category.Id, media.Id, "existing")); await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ValidationException>(() => new PostService(db).CreateAsync(CreateDto(category.Id, media.Id, slug: "existing")));
    }

    [Fact]
    public async Task CreateAsync_WhenValid_CreatesPostWithTagsAndReadingTime()
    {
        await using var db = CreateContext();
        var category = AddLeafCategory(); var media = AddMedia();
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "C#", Slug = "csharp" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "NET", Slug = "net" };
        db.AddRange(category, media, tag1, tag2); await db.SaveChangesAsync();

        var result = await new PostService(db).CreateAsync(CreateDto(category.Id, media.Id, tagIds: [tag1.Id, tag2.Id], content: string.Join(' ', Enumerable.Repeat("word", 201))));

        Assert.Equal("new-post", result.Slug);
        Assert.Equal(2, result.ReadingTimeMinutes);
        Assert.Equal(2, (await db.PostTags.CountAsync(x => x.PostId == result.Id)));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPropertiesAndSynchronizesTags()
    {
        await using var db = CreateContext();
        var category = AddLeafCategory(); var media = AddMedia();
        var oldTag = new Tag { Id = Guid.NewGuid(), Name = "Old", Slug = "old" };
        var newTag = new Tag { Id = Guid.NewGuid(), Name = "New", Slug = "new" };
        var post = CreatePost(category.Id, media.Id, "old-post"); post.PostTags.Add(new PostTag { TagId = oldTag.Id });
        db.AddRange(category, media, oldTag, newTag, post); await db.SaveChangesAsync();

        await new PostService(db).UpdateAsync(post.Id, new UpdatePostDto("Updated", "updated-post", "excerpt", "content", category.Id, media.Id, null, [newTag.Id], PostStatus.Hidden, 3, "meta", "description"));

        var updated = await db.Posts.Include(x => x.PostTags).SingleAsync(x => x.Id == post.Id);
        Assert.Equal("Updated", updated.Title); Assert.Equal("updated-post", updated.Slug); Assert.Equal(PostStatus.Hidden, updated.Status);
        Assert.Equal(3, updated.ReadingTimeMinutes); Assert.Equal([newTag.Id], updated.PostTags.Select(x => x.TagId));
    }

    [Fact]
    public async Task DeleteAsync_RemovesPost()
    {
        await using var db = CreateContext();
        var category = AddLeafCategory(); var media = AddMedia(); var post = CreatePost(category.Id, media.Id, "delete-me");
        db.AddRange(category, media, post); await db.SaveChangesAsync();

        await new PostService(db).DeleteAsync(post.Id);

        Assert.Null(await db.Posts.FindAsync(post.Id));
    }

    [Fact]
    public async Task GetFeaturedAsync_ReturnsOrderedByViewAndRating()
    {
        await using var db = CreateContext();
        var category = AddLeafCategory(); var media = AddMedia();
        var lowerRated = CreatePost(category.Id, media.Id, "lower", 10, 2m);
        var higherRated = CreatePost(category.Id, media.Id, "higher", 10, 4m);
        var lowerViews = CreatePost(category.Id, media.Id, "views", 9, 5m);
        db.AddRange(category, media, lowerRated, higherRated, lowerViews); await db.SaveChangesAsync();

        var featured = await new PostService(db).GetFeaturedAsync(3);

        Assert.Equal(["higher", "lower", "views"], featured.Select(x => x.Slug));
    }

    [Fact]
    public async Task GetBySlugAsync_WhenIncrementViewTrue_IncrementsViewCount()
    {
        await using var db = CreateContext();
        var category = AddLeafCategory(); var media = AddMedia(); var post = CreatePost(category.Id, media.Id, "view-me", 4);
        db.AddRange(category, media, post); await db.SaveChangesAsync();

        var result = await new PostService(db).GetBySlugAsync("view-me", true);

        Assert.NotNull(result); Assert.Equal(5, result.ViewCount); Assert.Equal(5, (await db.Posts.FindAsync(post.Id))!.ViewCount);
    }

    private static CreatePostDto CreateDto(Guid categoryId, Guid thumbnailId, string slug = "new-post", List<Guid>? tagIds = null, string? content = "content") => new(" New post ", slug, null, content, categoryId, thumbnailId, null, tagIds);
    private static Category AddLeafCategory() => new() { Id = Guid.NewGuid(), Name = "Leaf", Slug = "leaf", Level = 3 };
    private static MediaLibrary AddMedia() => new() { Id = Guid.NewGuid(), FileName = "image.png", Url = "/image.png", UsageType = MediaUsageType.PostThumbnail };
    private static Post CreatePost(Guid categoryId, Guid mediaId, string slug, int views = 0, decimal rating = 0m) => new() { Id = Guid.NewGuid(), CategoryId = categoryId, ThumbnailMediaId = mediaId, Title = slug, Slug = slug, Status = PostStatus.Published, ViewCount = views, AvgRating = rating, PublishedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
    private static TestApplicationDbContext CreateContext() => new(new DbContextOptionsBuilder<TestApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
