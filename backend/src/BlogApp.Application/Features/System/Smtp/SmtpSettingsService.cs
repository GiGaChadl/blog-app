using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.System.Smtp;

public class SmtpSettingsService
{
    private readonly IApplicationDbContext _db;
    private readonly IEncryptionService _encryptionService;
    private readonly IEmailSender _emailSender;

    public SmtpSettingsService(
        IApplicationDbContext db,
        IEncryptionService encryptionService,
        IEmailSender emailSender)
    {
        _db = db;
        _encryptionService = encryptionService;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Lấy cấu hình SMTP hiện tại (ẩn mật khẩu, chỉ báo HasPassword = true/false).
    /// </summary>
    public async Task<SmtpSettingsDto> GetSettingsAsync(CancellationToken ct = default)
    {
        var settings = await _db.SmtpSettings.FirstOrDefaultAsync(ct);
        if (settings == null)
        {
            return new SmtpSettingsDto(
                Id: Guid.Empty,
                Host: null,
                Port: null,
                Username: null,
                SenderEmail: null,
                SenderName: null,
                DailyLimit: 50,
                EmailsSentToday: 0,
                LastResetAt: null,
                HasPassword: false,
                EnableSsl: true
            );
        }

        return new SmtpSettingsDto(
            settings.Id,
            settings.Host,
            settings.Port,
            settings.Username,
            settings.SenderEmail,
            settings.SenderName,
            settings.DailyLimit,
            settings.EmailsSentToday,
            settings.LastResetAt,
            !string.IsNullOrWhiteSpace(settings.PasswordEncrypted),
            settings.Port == 465 || settings.Port == 587 || settings.Port == null
        );
    }

    /// <summary>
    /// Cập nhật cấu hình SMTP.
    /// Nếu có nhập mật khẩu mới, mã hoá 2 chiều bằng AES-256 trước khi lưu.
    /// Nếu để trống mật khẩu, giữ nguyên mật khẩu đã mã hoá cũ.
    /// </summary>
    public async Task<SmtpSettingsDto> UpdateSettingsAsync(UpdateSmtpSettingsDto dto, CancellationToken ct = default)
    {
        var settings = await _db.SmtpSettings.FirstOrDefaultAsync(ct);

        if (settings == null)
        {
            settings = new SmtpSettings
            {
                Id = Guid.NewGuid(),
                Host = dto.Host?.Trim(),
                Port = dto.Port,
                Username = dto.Username?.Trim(),
                PasswordEncrypted = string.IsNullOrWhiteSpace(dto.Password) ? null : _encryptionService.Encrypt(dto.Password),
                SenderEmail = dto.SenderEmail?.Trim(),
                SenderName = dto.SenderName?.Trim(),
                DailyLimit = dto.DailyLimit > 0 ? dto.DailyLimit : 50,
                EmailsSentToday = 0,
                LastResetAt = DateTime.UtcNow
            };
            _db.SmtpSettings.Add(settings);
        }
        else
        {
            settings.Host = dto.Host?.Trim();
            settings.Port = dto.Port;
            settings.Username = dto.Username?.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                settings.PasswordEncrypted = _encryptionService.Encrypt(dto.Password);
            }
            settings.SenderEmail = dto.SenderEmail?.Trim();
            settings.SenderName = dto.SenderName?.Trim();
            settings.DailyLimit = dto.DailyLimit > 0 ? dto.DailyLimit : 50;
        }

        await _db.SaveChangesAsync(ct);

        return new SmtpSettingsDto(
            settings.Id,
            settings.Host,
            settings.Port,
            settings.Username,
            settings.SenderEmail,
            settings.SenderName,
            settings.DailyLimit,
            settings.EmailsSentToday,
            settings.LastResetAt,
            !string.IsNullOrWhiteSpace(settings.PasswordEncrypted),
            dto.EnableSsl ?? (settings.Port == 465 || settings.Port == 587 || true)
        );
    }

    /// <summary>
    /// Thử nghiệm gửi email kiểm tra cấu hình SMTP hoạt động.
    /// </summary>
    public async Task TestSmtpConnectionAsync(TestSmtpRequest request, CancellationToken ct = default)
    {
        var recipient = request.ResolvedToEmail?.Trim();
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ValidationException("Email nhận thư thử nghiệm không được để trống.");

        var settings = await _db.SmtpSettings.FirstOrDefaultAsync(ct);
        if (settings == null || string.IsNullOrWhiteSpace(settings.Host))
            throw new ValidationException("Chưa cấu hình thông tin máy chủ SMTP.");

        var subject = "[Kiểm Tra] Thử nghiệm cấu hình SMTP thành công";
        var body = $@"
            <div style=""font-family: Arial, sans-serif; line-height: 1.6;"">
                <h2 style=""color: #16a34a;"">Chúc mừng! Cấu hình SMTP hoạt động chính xác</h2>
                <p>Email này được gửi thử nghiệm từ hệ thống BlogApp.</p>
                <ul>
                    <li>Máy chủ: <strong>{settings.Host}</strong></li>
                    <li>Cổng: <strong>{settings.Port}</strong></li>
                    <li>Tài khoản: <strong>{settings.Username}</strong></li>
                    <li>Tên người gửi: <strong>{settings.SenderName}</strong></li>
                </ul>
            </div>";

        await _emailSender.SendEmailAsync(recipient, subject, body, ct);
    }
}
