using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Common.Interfaces;

/// <summary>
/// Trừu tượng hoá ApplicationDbContext để Application layer không phụ thuộc trực tiếp vào
/// EF Core/Infrastructure — chỉ biết "có DbSet để query", không biết đó là Postgres hay gì khác.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<MediaLibrary> MediaLibrary { get; }
    DbSet<Category> Categories { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Post> Posts { get; }
    DbSet<PostTag> PostTags { get; }

    DbSet<ReviewSession> ReviewSessions { get; }
    DbSet<PostReview> PostReviews { get; }
    DbSet<OtpVerification> OtpVerifications { get; }
    DbSet<Subscriber> Subscribers { get; }

    DbSet<Banner> Banners { get; }
    DbSet<Slogan> Slogans { get; }
    DbSet<Ad> Ads { get; }
    DbSet<Project> Projects { get; }
    DbSet<AdminUser> AdminUsers { get; }
    DbSet<ContactInfo> ContactInfo { get; }

    DbSet<VisitorLog> VisitorLogs { get; }
    DbSet<VisitorStatsMonthly> VisitorStatsMonthly { get; }
    DbSet<CookieConsent> CookieConsents { get; }
    DbSet<SmtpSettings> SmtpSettings { get; }
    DbSet<SeoSettings> SeoSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
