using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> b)
    {
        b.ToTable("posts");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();
        b.Property(x => x.ThumbnailMediaId).HasColumnName("thumbnail_media_id").IsRequired();
        b.Property(x => x.OgImageMediaId).HasColumnName("og_image_media_id");
        b.Property(x => x.Title).HasColumnName("title").IsRequired();
        b.Property(x => x.Slug).HasColumnName("slug").IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Excerpt).HasColumnName("excerpt");
        b.Property(x => x.ContentHtml).HasColumnName("content_html");
        b.Property(x => x.Status).HasColumnName("status").IsRequired();
        b.Property(x => x.ViewCount).HasColumnName("view_count").HasDefaultValue(0);
        b.Property(x => x.AvgRating).HasColumnName("avg_rating").HasColumnType("decimal(2,1)").HasDefaultValue(0m);
        b.Property(x => x.RatingCount).HasColumnName("rating_count").HasDefaultValue(0);
        b.Property(x => x.ReadingTimeMinutes).HasColumnName("reading_time_minutes");
        b.Property(x => x.MetaTitle).HasColumnName("meta_title");
        b.Property(x => x.MetaDescription).HasColumnName("meta_description");
        b.Property(x => x.PublishedAt).HasColumnName("published_at").IsRequired().HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

        // category_id: RESTRICT — chặn xoá danh mục đang có bài viết.
        // LƯU Ý: EF KHÔNG thể tự validate "category phải là cấp 3" (đó là trigger DB
        // fn_check_post_category_level) — Application layer NÊN tự kiểm tra trước khi
        // gọi SaveChanges để trả lỗi thân thiện, thay vì để trigger ném exception thô.
        b.HasOne(x => x.Category)
            .WithMany(x => x.Posts)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ThumbnailMedia)
            .WithMany(x => x.ThumbnailForPosts)
            .HasForeignKey(x => x.ThumbnailMediaId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.OgImageMedia)
            .WithMany(x => x.OgImageForPosts)
            .HasForeignKey(x => x.OgImageMediaId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.CategoryId);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.PublishedAt);
    }
}
