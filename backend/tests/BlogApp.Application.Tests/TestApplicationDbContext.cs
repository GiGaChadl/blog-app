using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Tests;

internal class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options) : base(options) { }

    public DbSet<MediaLibrary> MediaLibrary => Set<MediaLibrary>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<ReviewSession> ReviewSessions => Set<ReviewSession>();
    public DbSet<PostReview> PostReviews => Set<PostReview>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<Slogan> Slogans => Set<Slogan>();
    public DbSet<Ad> Ads => Set<Ad>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<ContactInfo> ContactInfo => Set<ContactInfo>();
    public DbSet<VisitorLog> VisitorLogs => Set<VisitorLog>();
    public DbSet<VisitorStatsMonthly> VisitorStatsMonthly => Set<VisitorStatsMonthly>();
    public DbSet<CookieConsent> CookieConsents => Set<CookieConsent>();
    public DbSet<SmtpSettings> SmtpSettings => Set<SmtpSettings>();
    public DbSet<SeoSettings> SeoSettings => Set<SeoSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Post>(b =>
        {
            b.HasOne(x => x.ThumbnailMedia)
                .WithMany(x => x.ThumbnailForPosts)
                .HasForeignKey(x => x.ThumbnailMediaId);

            b.HasOne(x => x.OgImageMedia)
                .WithMany(x => x.OgImageForPosts)
                .HasForeignKey(x => x.OgImageMediaId);
        });

        modelBuilder.Entity<PostTag>(b =>
        {
            b.HasKey(x => new { x.PostId, x.TagId });
        });
    }
}
