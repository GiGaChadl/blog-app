namespace BlogApp.Domain.Entities;

/// <summary>Month luôn là ngày 01 của tháng đó (quy ước, VD 2026-08-01 = "tháng 8/2026").
/// Cron tổng hợp tháng cũ TRƯỚC khi xoá VisitorLog quá 30 ngày.</summary>
public class VisitorStatsMonthly
{
    public Guid Id { get; set; }
    public DateOnly Month { get; set; }
    public int TotalVisits { get; set; }
    public int UniqueVisitors { get; set; }
    public DateTime CreatedAt { get; set; }
}
