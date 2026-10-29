namespace BlogApp.Domain.Entities;

/// <summary>Bảng singleton. PasswordEncrypted dùng MÃ HOÁ 2 CHIỀU (khác hash) vì cần lấy lại
/// mật khẩu gốc để đăng nhập SMTP. Khoá mã hoá lưu ở biến môi trường, KHÔNG lưu trong DB.</summary>
public class SmtpSettings
{
    public Guid Id { get; set; }
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? PasswordEncrypted { get; set; }
    public string? SenderEmail { get; set; }
    public string? SenderName { get; set; }
    public int DailyLimit { get; set; } = 50;
    public int EmailsSentToday { get; set; }
    public DateTime? LastResetAt { get; set; }
}
