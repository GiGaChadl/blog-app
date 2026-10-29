using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Features.System.CookieConsent;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class CookieConsentServiceTests
{
    private (TestApplicationDbContext Db, CookieConsentService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var service = new CookieConsentService(db);

        return (db, service);
    }

    [Fact]
    public async Task RecordConsent_DeclinedFirstTime_DoesNotCreateDatabaseRecord()
    {
        var (db, service) = CreateService();

        var request = new RecordConsentRequest("token-first-time-declined", CookieChoice.Declined);
        var result = await service.RecordConsentAsync(request);

        Assert.False(result.HasConsented);
        Assert.Null(result.Choice);

        // Kiểm tra không có bản ghi nào trong DB (PROJECT-MASTER-GUIDE §10)
        var count = await db.CookieConsents.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task RecordConsent_AcceptedFirstTime_CreatesDatabaseRecord()
    {
        var (db, service) = CreateService();

        var request = new RecordConsentRequest("token-accepted-123", CookieChoice.Accepted);
        var result = await service.RecordConsentAsync(request);

        Assert.True(result.HasConsented);
        Assert.Equal(CookieChoice.Accepted, result.Choice);
        Assert.NotNull(result.ConsentedAt);

        // Kiểm tra DB có đúng 1 bản ghi
        var count = await db.CookieConsents.CountAsync();
        Assert.Equal(1, count);

        var hasAccepted = await service.HasAcceptedAsync("token-accepted-123");
        Assert.True(hasAccepted);
    }

    [Fact]
    public async Task RecordConsent_RevokeConsentLater_UpdatesDatabaseRecord()
    {
        var (db, service) = CreateService();

        // Bước 1: Đồng ý lần đầu
        await service.RecordConsentAsync(new RecordConsentRequest("token-flip", CookieChoice.Accepted));
        Assert.True(await service.HasAcceptedAsync("token-flip"));

        // Bước 2: Chủ động thu hồi (Declined)
        var result = await service.RecordConsentAsync(new RecordConsentRequest("token-flip", CookieChoice.Declined));
        Assert.True(result.HasConsented); // Đã từng tương tác
        Assert.Equal(CookieChoice.Declined, result.Choice);

        // DB vẫn chỉ có 1 bản ghi và trạng thái cập nhật sang Declined
        var count = await db.CookieConsents.CountAsync();
        Assert.Equal(1, count);

        var hasAccepted = await service.HasAcceptedAsync("token-flip");
        Assert.False(hasAccepted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RecordConsent_EmptyToken_ThrowsValidationException(string token)
    {
        var (_, service) = CreateService();

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.RecordConsentAsync(new RecordConsentRequest(token, CookieChoice.Accepted)));
    }
}
