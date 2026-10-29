using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class PostTagConfiguration : IEntityTypeConfiguration<PostTag>
{
    public void Configure(EntityTypeBuilder<PostTag> b)
    {
        b.ToTable("post_tags");
        b.HasKey(x => new { x.PostId, x.TagId });

        b.Property(x => x.PostId).HasColumnName("post_id");
        b.Property(x => x.TagId).HasColumnName("tag_id");

        // CASCADE cả 2 chiều — khác categories/posts.category_id (RESTRICT), vì đây là
        // bảng liên kết thuần, không phải nội dung chính cần bảo vệ.
        b.HasOne(x => x.Post)
            .WithMany(x => x.PostTags)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Tag)
            .WithMany(x => x.PostTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
