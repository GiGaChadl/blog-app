namespace BlogApp.Application.Features.System.Smtp;

public record SmtpSettingsDto(
    Guid Id,
    string? Host,
    int? Port,
    string? Username,
    string? SenderEmail,
    string? SenderName,
    int DailyLimit,
    int EmailsSentToday,
    DateTime? LastResetAt,
    bool HasPassword,
    bool EnableSsl = true
);

public record UpdateSmtpSettingsDto(
    string? Host,
    int? Port,
    string? Username,
    string? Password,
    string? SenderEmail,
    string? SenderName,
    int DailyLimit = 50,
    bool? EnableSsl = true
);

public record TestSmtpRequest(
    string? ToEmail = null,
    string? TestRecipientEmail = null
)
{
    public string ResolvedToEmail => !string.IsNullOrWhiteSpace(ToEmail) ? ToEmail : (TestRecipientEmail ?? string.Empty);
}
