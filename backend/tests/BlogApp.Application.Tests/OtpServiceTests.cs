using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Features.Interactions.Otp;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class OtpServiceTests
{
    private (TestApplicationDbContext Db, FakeEmailSender EmailSender, OtpService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var emailSender = new FakeEmailSender();
        var service = new OtpService(db, emailSender);

        return (db, emailSender, service);
    }

    [Fact]
    public async Task SendReviewOtp_ValidRequest_CreatesRecordAndDispatchesEmail()
    {
        var (db, emailSender, service) = CreateService();

        var request = new SendReviewOtpRequest("reader@example.com");
        await service.SendReviewOtpAsync(request, "192.168.1.100");

        var otpRecord = await db.OtpVerifications.FirstOrDefaultAsync();
        Assert.NotNull(otpRecord);
        Assert.Equal("reader@example.com", otpRecord.Email);
        Assert.Equal("192.168.1.100", otpRecord.IpAddress);
        Assert.Equal(OtpPurpose.ReviewVerification, otpRecord.Purpose);
        Assert.False(otpRecord.IsUsed);

        Assert.Single(emailSender.SentEmails);
        Assert.Equal("reader@example.com", emailSender.SentEmails[0].To);
        Assert.Contains("xác thực", emailSender.SentEmails[0].Subject, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing-at.com")]
    public async Task SendReviewOtp_InvalidEmail_ThrowsValidationException(string email)
    {
        var (_, _, service) = CreateService();

        var request = new SendReviewOtpRequest(email);
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SendReviewOtpAsync(request, "127.0.0.1"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendReviewOtp_MissingIpAddress_ThrowsValidationException(string? ip)
    {
        var (_, _, service) = CreateService();

        var request = new SendReviewOtpRequest("test@example.com");
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SendReviewOtpAsync(request, ip));
    }

    [Fact]
    public async Task SendReviewOtp_Exceeds5TimesIn24h_ThrowsRateLimitException()
    {
        var (db, _, service) = CreateService();
        var email = "spam@example.com";

        // Tạo sẵn 5 lần gửi trong 24 giờ qua
        for (int i = 0; i < 5; i++)
        {
            db.OtpVerifications.Add(new OtpVerification
            {
                Id = Guid.NewGuid(),
                Email = email,
                OtpCodeHash = "hash_" + i,
                Purpose = OtpPurpose.ReviewVerification,
                IpAddress = "10.0.0.1",
                CreatedAt = DateTime.UtcNow.AddMinutes(-30 * i),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            });
        }
        await db.SaveChangesAsync();

        // Lần thứ 6 phải bị chặn
        var request = new SendReviewOtpRequest(email);
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            service.SendReviewOtpAsync(request, "10.0.0.1"));

        Assert.Contains("quá số lần cho phép", ex.Message);
    }

    [Fact]
    public async Task VerifyReviewOtp_ValidOtp_CreatesSessionAndMarksOtpUsed()
    {
        var (db, emailSender, service) = CreateService();
        var email = "user@test.com";

        // Gửi OTP
        await service.SendReviewOtpAsync(new SendReviewOtpRequest(email), "1.2.3.4");

        var otpRecord = await db.OtpVerifications.FirstAsync();
        // Trích xuất mã OTP từ email body
        var emailBody = emailSender.SentEmails[0].Body;
        // Mã OTP là 6 số nằm trong html
        var match = System.Text.RegularExpressions.Regex.Match(emailBody, @"\b(\d{6})\b");
        Assert.True(match.Success);
        var otpCode = match.Value;

        // Xác thực
        var verifyResult = await service.VerifyReviewOtpAsync(new VerifyReviewOtpRequest(email, otpCode));

        Assert.NotNull(verifyResult);
        Assert.NotEqual(Guid.Empty, verifyResult.SessionId);
        Assert.Equal(email, verifyResult.Email);
        Assert.True(verifyResult.ExpiresAt > DateTime.UtcNow.AddHours(23));

        // Kiểm tra record DB
        await db.Entry(otpRecord).ReloadAsync();
        Assert.True(otpRecord.IsUsed);

        var session = await db.ReviewSessions.FirstOrDefaultAsync(s => s.Id == verifyResult.SessionId);
        Assert.NotNull(session);
        Assert.Equal(email, session.Email);
    }

    [Fact]
    public async Task VerifyReviewOtp_WrongOtpCode_ThrowsValidationException()
    {
        var (db, _, service) = CreateService();
        var email = "user@test.com";

        await service.SendReviewOtpAsync(new SendReviewOtpRequest(email), "1.2.3.4");

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.VerifyReviewOtpAsync(new VerifyReviewOtpRequest(email, "999999")));
    }

    [Fact]
    public async Task VerifyReviewOtp_ExpiredOtp_ThrowsValidationException()
    {
        var (db, _, service) = CreateService();
        var email = "expired@test.com";

        // Tạo OTP hết hạn
        db.OtpVerifications.Add(new OtpVerification
        {
            Id = Guid.NewGuid(),
            Email = email,
            OtpCodeHash = "somehash",
            Purpose = OtpPurpose.ReviewVerification,
            IpAddress = "1.2.3.4",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-20),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10) // Đã hết hạn
        });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.VerifyReviewOtpAsync(new VerifyReviewOtpRequest(email, "123456")));
    }
}
