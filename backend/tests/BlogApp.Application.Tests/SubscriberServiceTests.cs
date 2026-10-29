using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Features.Interactions.Subscribers;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class SubscriberServiceTests
{
    private (TestApplicationDbContext Db, FakeEmailSender EmailSender, SubscriberService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var emailSender = new FakeEmailSender();
        var service = new SubscriberService(db, emailSender);

        return (db, emailSender, service);
    }

    [Fact]
    public async Task Subscribe_NewEmail_CreatesPendingSubscriberAndSendsEmail()
    {
        var (db, emailSender, service) = CreateService();
        var email = "reader@domain.com";

        await service.SubscribeAsync(new SubscribeRequest(email), "http://localhost:5000");

        var subscriber = await db.Subscribers.FirstOrDefaultAsync(s => s.Email == email);
        Assert.NotNull(subscriber);
        Assert.Equal(SubscriberStatus.Pending, subscriber.Status);
        Assert.False(string.IsNullOrWhiteSpace(subscriber.ConfirmToken));
        Assert.True(subscriber.ConfirmTokenExpiresAt > DateTime.UtcNow.AddDays(6));

        Assert.Single(emailSender.SentEmails);
        Assert.Equal(email, emailSender.SentEmails[0].To);
        Assert.Contains(subscriber.ConfirmToken, emailSender.SentEmails[0].Body);
    }

    [Fact]
    public async Task Subscribe_ExistingActiveEmail_ThrowsValidationException()
    {
        var (db, _, service) = CreateService();
        var email = "active@domain.com";

        db.Subscribers.Add(new Subscriber
        {
            Id = Guid.NewGuid(),
            Email = email,
            Status = SubscriberStatus.Active,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SubscribeAsync(new SubscribeRequest(email), "http://localhost:5000"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    public async Task Subscribe_InvalidEmail_ThrowsValidationException(string email)
    {
        var (_, _, service) = CreateService();

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SubscribeAsync(new SubscribeRequest(email), null));
    }

    [Fact]
    public async Task ConfirmSubscription_ValidToken_ActivatesSubscriber()
    {
        var (db, _, service) = CreateService();
        var token = "valid_token_123";

        var subscriber = new Subscriber
        {
            Id = Guid.NewGuid(),
            Email = "subscriber@test.com",
            Status = SubscriberStatus.Pending,
            ConfirmToken = token,
            ConfirmTokenExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        db.Subscribers.Add(subscriber);
        await db.SaveChangesAsync();

        await service.ConfirmSubscriptionAsync(token);

        await db.Entry(subscriber).ReloadAsync();
        Assert.Equal(SubscriberStatus.Active, subscriber.Status);
        Assert.NotNull(subscriber.ConfirmedAt);
        Assert.Null(subscriber.ConfirmToken);
        Assert.Null(subscriber.ConfirmTokenExpiresAt);
    }

    [Fact]
    public async Task ConfirmSubscription_ExpiredToken_ThrowsValidationException()
    {
        var (db, _, service) = CreateService();
        var token = "expired_token_456";

        var subscriber = new Subscriber
        {
            Id = Guid.NewGuid(),
            Email = "expired@test.com",
            Status = SubscriberStatus.Pending,
            ConfirmToken = token,
            ConfirmTokenExpiresAt = DateTime.UtcNow.AddDays(-1), // Đã hết hạn
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };
        db.Subscribers.Add(subscriber);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.ConfirmSubscriptionAsync(token));
    }

    [Fact]
    public async Task Unsubscribe_ActiveSubscriber_MarksStatusUnsubscribed()
    {
        var (db, _, service) = CreateService();
        var email = "cancel@test.com";

        var subscriber = new Subscriber
        {
            Id = Guid.NewGuid(),
            Email = email,
            Status = SubscriberStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        db.Subscribers.Add(subscriber);
        await db.SaveChangesAsync();

        await service.UnsubscribeAsync(new UnsubscribeRequest(email));

        await db.Entry(subscriber).ReloadAsync();
        Assert.Equal(SubscriberStatus.Unsubscribed, subscriber.Status);
        Assert.NotNull(subscriber.UnsubscribedAt);
    }
}
