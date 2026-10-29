using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Infrastructure.Persistence;

/// <summary>
/// DbContext MAP VÀO schema đã có sẵn (tạo bởi blog-database-schema.sql, đã test thật bằng
/// 10 tình huống — xem changelog). Layer này KHÔNG chịu trách nhiệm tạo/sửa schema qua EF
/// Core Migrations ở giai đoạn hiện tại — mọi trigger, CHECK phức tạp (categories.level,
/// posts.category_id phải cấp 3, otp ip_address theo purpose), và partial unique index
/// (slogans) đã có sẵn trong file SQL đó. Các IEntityTypeConfiguration bên dưới chỉ khai báo
/// lại một phần các ràng buộc này (CHECK, index) để mô hình EF khớp với thực tế DB — không
/// phải để EF tự sinh ra chúng.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Nhóm 1 — Nội dung
    public DbSet<MediaLibrary> MediaLibrary => Set<MediaLibrary>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostTag> PostTags => Set<PostTag>();

    // Nhóm 2 — Tương tác
    public DbSet<ReviewSession> ReviewSessions => Set<ReviewSession>();
    public DbSet<PostReview> PostReviews => Set<PostReview>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();

    // Nhóm 3 — Quản trị
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<Slogan> Slogans => Set<Slogan>();
    public DbSet<Ad> Ads => Set<Ad>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<ContactInfo> ContactInfo => Set<ContactInfo>();

    // Nhóm 4 — Hệ thống
    public DbSet<VisitorLog> VisitorLogs => Set<VisitorLog>();
    public DbSet<VisitorStatsMonthly> VisitorStatsMonthly => Set<VisitorStatsMonthly>();
    public DbSet<CookieConsent> CookieConsents => Set<CookieConsent>();
    public DbSet<SmtpSettings> SmtpSettings => Set<SmtpSettings>();
    public DbSet<SeoSettings> SeoSettings => Set<SeoSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Khai báo native PostgreSQL ENUM TYPE cho từng cột enum — PHẢI khớp đúng tên type
        // đã CREATE TYPE trong blog-database-schema.sql (post_status, media_usage_type, ...).
        modelBuilder.HasPostgresEnum<Domain.Enums.PostStatus>("post_status");
        modelBuilder.HasPostgresEnum<Domain.Enums.MediaUsageType>("media_usage_type");
        modelBuilder.HasPostgresEnum<Domain.Enums.BannerType>("banner_type");
        modelBuilder.HasPostgresEnum<Domain.Enums.AdPosition>("ad_position");
        modelBuilder.HasPostgresEnum<Domain.Enums.ReviewStatus>("review_status");
        modelBuilder.HasPostgresEnum<Domain.Enums.SubscriberStatus>("subscriber_status");
        modelBuilder.HasPostgresEnum<Domain.Enums.OtpPurpose>("otp_purpose");
        modelBuilder.HasPostgresEnum<Domain.Enums.CookieChoice>("cookie_choice");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
