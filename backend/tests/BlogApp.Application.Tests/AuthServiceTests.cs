using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Features.Auth;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) => "hashed_" + plainPassword;
    public bool Verify(string plainPassword, string hash) => hash == "hashed_" + plainPassword;
}

public class FakeJwtTokenService : IJwtTokenService
{
    public string GenerateToken(AdminUser user) => "fake_jwt_token";
}

public class AuthServiceTests
{
    private (TestApplicationDbContext Db, FakeEmailSender EmailSender, FakePasswordHasher Hasher, AuthService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var emailSender = new FakeEmailSender();
        var hasher = new FakePasswordHasher();
        var jwt = new FakeJwtTokenService();
        var service = new AuthService(db, hasher, jwt, emailSender);

        return (db, emailSender, hasher, service);
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidCurrentPassword_ChangesPasswordSuccessfully()
    {
        var (db, _, hasher, service) = CreateService();

        var admin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = hasher.Hash("OldPass123!"),
            Email = "admin@blog.local",
            CreatedAt = DateTime.UtcNow
        };
        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();

        var request = new ChangePasswordRequestDto("OldPass123!", "NewSecurePassword456!");
        await service.ChangePasswordAsync("admin", request);

        await db.Entry(admin).ReloadAsync();
        Assert.True(hasher.Verify("NewSecurePassword456!", admin.PasswordHash));
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ThrowsValidationException()
    {
        var (db, _, hasher, service) = CreateService();

        var admin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = hasher.Hash("RightPass123!"),
            Email = "admin@blog.local",
            CreatedAt = DateTime.UtcNow
        };
        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();

        var request = new ChangePasswordRequestDto("WrongPass!", "NewPassword123!");
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            service.ChangePasswordAsync("admin", request));

        Assert.Contains("không chính xác", ex.Message);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShortNewPassword_ThrowsValidationException()
    {
        var (db, _, hasher, service) = CreateService();

        var admin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = hasher.Hash("Pass123!"),
            Email = "admin@blog.local",
            CreatedAt = DateTime.UtcNow
        };
        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();

        var request = new ChangePasswordRequestDto("Pass123!", "123"); // Ngắn hơn 6 ký tự
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.ChangePasswordAsync("admin", request));
    }

    [Fact]
    public async Task SendForgotPasswordOtpAsync_ValidAdminEmail_DispatchesOtp()
    {
        var (db, emailSender, hasher, service) = CreateService();

        var admin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = hasher.Hash("Pass123!"),
            Email = "admin@blog.local",
            CreatedAt = DateTime.UtcNow
        };
        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();

        await service.SendForgotPasswordOtpAsync("admin@blog.local");

        var otp = await db.OtpVerifications.FirstOrDefaultAsync(o => o.Purpose == OtpPurpose.AdminPasswordReset);
        Assert.NotNull(otp);
        Assert.Equal("admin@blog.local", otp.Email);
        Assert.Null(otp.IpAddress); // Đúng chuẩn CHECK constraint không ép IP với admin reset
        Assert.False(otp.IsUsed);

        Assert.Single(emailSender.SentEmails);
        Assert.Equal("admin@blog.local", emailSender.SentEmails[0].To);
    }

    [Fact]
    public async Task SendForgotPasswordOtpAsync_NonAdminEmail_ThrowsValidationException()
    {
        var (_, _, _, service) = CreateService();

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SendForgotPasswordOtpAsync("stranger@hacker.com"));
    }

    [Fact]
    public async Task ResetPasswordWithOtpAsync_ValidOtp_UpdatesAdminPassword()
    {
        var (db, emailSender, hasher, service) = CreateService();

        var admin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = hasher.Hash("OldPass123!"),
            Email = "admin@blog.local",
            CreatedAt = DateTime.UtcNow
        };
        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();

        // Gửi OTP
        await service.SendForgotPasswordOtpAsync("admin@blog.local");

        var emailBody = emailSender.SentEmails[0].Body;
        var match = System.Text.RegularExpressions.Regex.Match(emailBody, @"\b(\d{6})\b");
        Assert.True(match.Success);
        var otpCode = match.Value;

        // Reset password
        var request = new ResetPasswordRequestDto("admin@blog.local", otpCode, "BrandNewPassword123!");
        await service.ResetPasswordWithOtpAsync(request);

        // Kiểm tra mật khẩu mới
        await db.Entry(admin).ReloadAsync();
        Assert.True(hasher.Verify("BrandNewPassword123!", admin.PasswordHash));

        // OTP phải được đánh dấu đã dùng
        var otpRecord = await db.OtpVerifications.FirstAsync(o => o.Purpose == OtpPurpose.AdminPasswordReset);
        Assert.True(otpRecord.IsUsed);
    }
}
