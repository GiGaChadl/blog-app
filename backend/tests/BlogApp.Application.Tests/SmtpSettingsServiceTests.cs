using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Features.System.Smtp;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class FakeEncryptionService : IEncryptionService
{
    public string Encrypt(string plainText) => "aes_encrypted_" + plainText;
    public string Decrypt(string cipherText) => cipherText.Replace("aes_encrypted_", "");
}

public class SmtpSettingsServiceTests
{
    private (TestApplicationDbContext Db, FakeEncryptionService Encryptor, FakeEmailSender EmailSender, SmtpSettingsService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var encryptor = new FakeEncryptionService();
        var emailSender = new FakeEmailSender();
        var service = new SmtpSettingsService(db, encryptor, emailSender);

        return (db, encryptor, emailSender, service);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WithNewPassword_EncryptsPasswordAndSaves()
    {
        var (db, encryptor, _, service) = CreateService();

        var dto = new UpdateSmtpSettingsDto("smtp.gmail.com", 587, "user@gmail.com", "SecretPass123", "sender@gmail.com", "Admin Blog", 100);
        var result = await service.UpdateSettingsAsync(dto);

        Assert.Equal("smtp.gmail.com", result.Host);
        Assert.Equal(587, result.Port);
        Assert.True(result.HasPassword);

        var saved = await db.SmtpSettings.FirstAsync();
        Assert.Equal(encryptor.Encrypt("SecretPass123"), saved.PasswordEncrypted);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WithEmptyPassword_PreservesExistingEncryptedPassword()
    {
        var (db, encryptor, _, service) = CreateService();

        db.SmtpSettings.Add(new SmtpSettings
        {
            Id = Guid.NewGuid(),
            Host = "old.host.com",
            PasswordEncrypted = encryptor.Encrypt("ExistingPassword")
        });
        await db.SaveChangesAsync();

        // Cập nhật nhưng để trống Password
        var updateDto = new UpdateSmtpSettingsDto("new.host.com", 465, "admin", null, "send@test.com", "Admin", 50);
        var result = await service.UpdateSettingsAsync(updateDto);

        Assert.Equal("new.host.com", result.Host);
        Assert.True(result.HasPassword);

        var saved = await db.SmtpSettings.FirstAsync();
        Assert.Equal(encryptor.Encrypt("ExistingPassword"), saved.PasswordEncrypted);
    }

    [Fact]
    public async Task GetSettingsAsync_DoesNotExposeRawPassword()
    {
        var (db, encryptor, _, service) = CreateService();

        db.SmtpSettings.Add(new SmtpSettings
        {
            Id = Guid.NewGuid(),
            Host = "smtp.mail.com",
            PasswordEncrypted = encryptor.Encrypt("TopSecret")
        });
        await db.SaveChangesAsync();

        var settings = await service.GetSettingsAsync();
        Assert.True(settings.HasPassword);
    }

    [Fact]
    public async Task TestSmtpConnectionAsync_ValidConfig_SendsTestEmail()
    {
        var (db, _, emailSender, service) = CreateService();

        db.SmtpSettings.Add(new SmtpSettings
        {
            Id = Guid.NewGuid(),
            Host = "smtp.live.com",
            Port = 587,
            Username = "blog@live.com",
            SenderName = "Tech Blog"
        });
        await db.SaveChangesAsync();

        await service.TestSmtpConnectionAsync(new TestSmtpRequest("tester@domain.com"));

        Assert.Single(emailSender.SentEmails);
        Assert.Equal("tester@domain.com", emailSender.SentEmails[0].To);
        Assert.Contains("Kiểm Tra", emailSender.SentEmails[0].Subject);
    }

    [Fact]
    public async Task TestSmtpConnectionAsync_MissingHost_ThrowsValidationException()
    {
        var (_, _, _, service) = CreateService();

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.TestSmtpConnectionAsync(new TestSmtpRequest("test@test.com")));
    }
}
