using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class PostReviewConfiguration : IEntityTypeConfiguration<PostReview>
{
    public void Configure(EntityTypeBuilder<PostReview> b)
    {
        b.ToTable("post_reviews", tb => tb.HasCheckConstraint(
            "post_reviews_rating_check", "rating BETWEEN 1 AND 5"));
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.PostId).HasColumnName("post_id").IsRequired();
        b.Property(x => x.SessionId).HasColumnName("session_id");
        b.Property(x => x.Email).HasColumnName("email").IsRequired();
        b.Property(x => x.Rating).HasColumnName("rating").IsRequired();
        b.Property(x => x.Comment).HasColumnName("comment");
        b.Property(x => x.Status).HasColumnName("status").IsRequired();
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

        // post_id: CASCADE — xoá bài viết thì xoá luôn review, không chặn xoá bài viết.
        b.HasOne(x => x.Post)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // session_id: SET NULL — cron dọn session hết hạn không được kéo theo mất review.
        b.HasOne(x => x.Session)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => x.PostId);
        b.HasIndex(x => x.SessionId);
        b.HasIndex(x => x.Status);
    }
}
