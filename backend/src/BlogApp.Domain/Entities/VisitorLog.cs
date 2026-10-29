namespace BlogApp.Domain.Entities;

/// <summary>Chỉ insert khi client đồng ý cookie (kiểm tra ở Application, không FK ràng buộc).
/// Giữ 30 ngày — cron xoá dòng cũ hơn, PHẢI chạy SAU job tổng hợp VisitorStatsMonthly.</summary>
public class VisitorLog
{
    public Guid Id { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceType { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PageUrl { get; set; }
    public string? Referrer { get; set; }
    public DateTime VisitedAt { get; set; }
}
