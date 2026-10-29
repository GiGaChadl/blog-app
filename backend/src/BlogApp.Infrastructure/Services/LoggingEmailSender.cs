using BlogApp.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BlogApp.Infrastructure.Services;

/// <summary>
/// Cài đặt IEmailSender dạng ghi log, phục vụ môi trường Dev/Staging và khi chưa kết nối SMTP thật.
/// Giúp hệ thống hoạt động thông suốt và dễ dàng debug nội dung OTP / Token xác nhận.
/// </summary>
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        _logger.LogInformation("================== [EMAIL DISPATCHED] ==================");
        _logger.LogInformation("To: {ToEmail}", toEmail);
        _logger.LogInformation("Subject: {Subject}", subject);
        _logger.LogInformation("Body: {HtmlBody}", htmlBody);
        _logger.LogInformation("========================================================");

        return Task.CompletedTask;
    }
}
