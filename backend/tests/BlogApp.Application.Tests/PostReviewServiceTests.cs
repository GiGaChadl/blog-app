using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Features.Interactions.Reviews;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class PostReviewServiceTests
{
    private (TestApplicationDbContext Db, PostReviewService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var service = new PostReviewService(db);

        return (db, service);
    }

    [Fact]
    public async Task CreateReview_ValidRequest_SavesReviewAndRefreshesPostRatingStats()
    {
        var (db, service) = CreateService();

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Bài viết hay",
            Slug = "bai-viet-hay",
            Status = PostStatus.Published,
            PublishedAt = DateTime.UtcNow,
            AvgRating = 0,
            RatingCount = 0
        };
        db.Posts.Add(post);

        var session = new ReviewSession
        {
            Id = Guid.NewGuid(),
            Email = "reader@domain.com",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        db.ReviewSessions.Add(session);
        await db.SaveChangesAsync();

        var dto = new CreateReviewDto(Rating: 5, Comment: "Rất bổ ích!", SessionId: session.Id);
        var result = await service.CreateReviewAsync(post.Id, dto, null);

        Assert.NotNull(result);
        Assert.Equal(5, result.Rating);
        Assert.Equal("re***@domain.com", result.MaskedEmail);
        Assert.Equal(ReviewStatus.Hidden, result.Status);

        // Ban đầu chưa duyệt thì chưa tính vào stats bài viết
        var postBeforeApproval = await db.Posts.FirstAsync(p => p.Id == post.Id);
        Assert.Equal(0, postBeforeApproval.RatingCount);
        Assert.Equal(0.0m, postBeforeApproval.AvgRating);

        // Quản trị viên duyệt đánh giá
        await service.UpdateStatusAsync(result.Id, ReviewStatus.Visible);

        var updatedPost = await db.Posts.FirstAsync(p => p.Id == post.Id);
        Assert.Equal(1, updatedPost.RatingCount);
        Assert.Equal(5.0m, updatedPost.AvgRating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(10)]
    public async Task CreateReview_InvalidRating_ThrowsValidationException(int rating)
    {
        var (db, service) = CreateService();

        var postId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var dto = new CreateReviewDto(Rating: rating, Comment: "Rating sai", SessionId: sessionId);
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateReviewAsync(postId, dto, null));

        Assert.Contains("từ 1 đến 5 sao", ex.Message);
    }

    [Fact]
    public async Task CreateReview_ExpiredSession_ThrowsValidationException()
    {
        var (db, service) = CreateService();

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Post Test",
            Slug = "post-test",
            Status = PostStatus.Published,
            PublishedAt = DateTime.UtcNow
        };
        db.Posts.Add(post);

        var expiredSession = new ReviewSession
        {
            Id = Guid.NewGuid(),
            Email = "expired@domain.com",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(-1) // Đã hết hạn
        };
        db.ReviewSessions.Add(expiredSession);
        await db.SaveChangesAsync();

        var dto = new CreateReviewDto(Rating: 4, Comment: "OK", SessionId: expiredSession.Id);
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateReviewAsync(post.Id, dto, null));
    }

    [Fact]
    public async Task GetPostReviews_ReturnsVisibleOnlyAndMasksEmail()
    {
        var (db, service) = CreateService();
        var postId = Guid.NewGuid();

        db.PostReviews.AddRange(
            new PostReview
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                Email = "nguyenvana@gmail.com",
                Rating = 5,
                Comment = "Tuyệt vời",
                Status = ReviewStatus.Visible,
                CreatedAt = DateTime.UtcNow
            },
            new PostReview
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                Email = "spammer@gmail.com",
                Rating = 1,
                Comment = "Spam",
                Status = ReviewStatus.Hidden, // Bị ẩn
                CreatedAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        var result = await service.GetPostReviewsAsync(postId, 1, 10);

        Assert.Equal(1, result.Total);
        Assert.Single(result.Items);
        Assert.Equal("ng***@gmail.com", result.Items[0].MaskedEmail);
        Assert.Equal(5, result.Items[0].Rating);
    }

    [Fact]
    public async Task UpdateReview_ByOwner_UpdatesContentAndStats()
    {
        var (db, service) = CreateService();

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Post",
            Slug = "post",
            Status = PostStatus.Published,
            PublishedAt = DateTime.UtcNow,
            AvgRating = 5,
            RatingCount = 1
        };
        db.Posts.Add(post);

        var session = new ReviewSession
        {
            Id = Guid.NewGuid(),
            Email = "author@gmail.com",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        db.ReviewSessions.Add(session);

        var review = new PostReview
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            SessionId = session.Id,
            Email = "author@gmail.com",
            Rating = 5,
            Comment = "Ban đầu 5 sao",
            Status = ReviewStatus.Visible,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.PostReviews.Add(review);
        await db.SaveChangesAsync();

        // Đổi rating từ 5 sao xuống 3 sao
        var updateDto = new UpdateReviewDto(Rating: 3, Comment: "Sửa lại 3 sao", SessionId: session.Id);
        var updated = await service.UpdateReviewAsync(review.Id, updateDto, null);

        Assert.Equal(3, updated.Rating);
        Assert.Equal("Sửa lại 3 sao", updated.Comment);

        var refreshedPost = await db.Posts.FirstAsync(p => p.Id == post.Id);
        Assert.Equal(3.0m, refreshedPost.AvgRating);
    }

    [Fact]
    public async Task UpdateReview_DifferentOwner_ThrowsValidationException()
    {
        var (db, service) = CreateService();

        var otherSession = new ReviewSession
        {
            Id = Guid.NewGuid(),
            Email = "hacker@gmail.com", // Khác email chủ sở hữu
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        db.ReviewSessions.Add(otherSession);

        var review = new PostReview
        {
            Id = Guid.NewGuid(),
            PostId = Guid.NewGuid(),
            Email = "legit_user@gmail.com",
            Rating = 5,
            Status = ReviewStatus.Visible,
            CreatedAt = DateTime.UtcNow
        };
        db.PostReviews.Add(review);
        await db.SaveChangesAsync();

        var updateDto = new UpdateReviewDto(Rating: 1, Comment: "Phá hoại", SessionId: otherSession.Id);
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateReviewAsync(review.Id, updateDto, null));
    }
}
