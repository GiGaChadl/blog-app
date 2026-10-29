using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class SeoSettingsConfiguration : IEntityTypeConfiguration<SeoSettings>
{
    public void Configure(EntityTypeBuilder<SeoSettings> b)
    {
        b.ToTable("seo_settings");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.DefaultMetaTitle).HasColumnName("default_meta_title");
        b.Property(x => x.DefaultMetaDescription).HasColumnName("default_meta_description");
        b.Property(x => x.RobotsTxtContent).HasColumnName("robots_txt_content");
        b.Property(x => x.SitemapGeneratedAt).HasColumnName("sitemap_generated_at");
    }
}
