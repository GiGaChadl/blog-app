using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BlogApp.Infrastructure.Services;

/// <summary>
/// Gửi email thật sự qua giao thức SMTP sử dụng MailKit.
/// Tự động hỗ trợ cả cổng 465 (Implicit SSL/SMTPS) và 587 (Explicit TLS/STARTTLS).
/// Nếu chưa cấu hình máy chủ SMTP, hệ thống sẽ ghi log an toàn mà không làm gián đoạn ứng dụng.
/// </summary>
public class MailKitEmailSender : IEmailSender
{
    private readonly IApplicationDbContext _db;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(
        IApplicationDbContext db,
        IEncryptionService encryptionService,
        ILogger<MailKitEmailSender> logger)
    {
        _db = db;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        _logger.LogInformation("================== [EMAIL DISPATCHED] ==================");
        _logger.LogInformation("To: {ToEmail}", toEmail);
        _logger.LogInformation("Subject: {Subject}", subject);
        _logger.LogInformation("========================================================");

        var settings = await _db.SmtpSettings.AsNoTracking().FirstOrDefaultAsync(ct);
        if (settings == null || string.IsNullOrWhiteSpace(settings.Host))
        {
            _logger.LogWarning("Chưa cấu hình máy chủ SMTP trong cơ sở dữ liệu. Bỏ qua gửi email thật.");
            return;
        }

        string? decryptedPassword = null;
        if (!string.IsNullOrWhiteSpace(settings.PasswordEncrypted))
        {
            try
            {
                decryptedPassword = _encryptionService.Decrypt(settings.PasswordEncrypted);
            }
            catch
            {
                decryptedPassword = settings.PasswordEncrypted;
            }
        }

        var port = settings.Port ?? 587;
        var socketOptions = port switch
        {
            465 => SecureSocketOptions.SslOnConnect,
            587 => SecureSocketOptions.StartTls,
            25 => SecureSocketOptions.StartTlsWhenAvailable,
            _ => SecureSocketOptions.Auto
        };

        var message = new MimeMessage();
        var fromAddress = !string.IsNullOrWhiteSpace(settings.SenderEmail) ? settings.SenderEmail : (settings.Username ?? "noreply@blogapp.local");
        var fromDisplay = !string.IsNullOrWhiteSpace(settings.SenderName) ? settings.SenderName : "BlogApp";
        message.From.Add(new MailboxAddress(fromDisplay, fromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            client.Timeout = 15000;
            // Bỏ qua kiểm tra chứng chỉ SSL nếu chạy localhost/self-signed
            client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            await client.ConnectAsync(settings.Host, port, socketOptions, ct);

            if (!string.IsNullOrWhiteSpace(settings.Username))
            {
                await client.AuthenticateAsync(settings.Username, decryptedPassword ?? string.Empty, ct);
            }

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
            _logger.LogInformation("Gửi email thành công tới {ToEmail} qua {Host}:{Port}", toEmail, settings.Host, port);
        }
        catch (AuthenticationException authEx)
        {
            _logger.LogError(authEx, "Xác thực SMTP thất bại đối với tài khoản {Username}", settings.Username);
            throw new ValidationException($"Xác thực tài khoản SMTP thất bại: Vui lòng kiểm tra lại tài khoản và mật khẩu ứng dụng (App Password 16 ký tự của Gmail nếu bật 2FA) cho máy chủ {settings.Host}.");
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            _logger.LogError(ex, "Lỗi khi gửi email qua máy chủ SMTP {Host}:{Port}", settings.Host, port);
            throw new ValidationException($"Lỗi gửi email qua máy chủ SMTP ({settings.Host}:{port}): {ex.Message}");
        }
    }
}
