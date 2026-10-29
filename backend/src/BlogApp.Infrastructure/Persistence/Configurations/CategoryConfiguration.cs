using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("categories");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Name).HasColumnName("name").IsRequired();
        b.Property(x => x.Slug).HasColumnName("slug").IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.ParentId).HasColumnName("parent_id");
        // level: NOT NULL nhưng KHÔNG set giá trị mặc định ở EF — trigger DB
        // (fn_set_category_level) tự tính, EF chỉ đọc lại giá trị sau khi insert.
        b.Property(x => x.Level).HasColumnName("level").IsRequired().ValueGeneratedOnAddOrUpdate();
        b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

        // Self-reference — ON DELETE RESTRICT (chặn xoá cha đang có con)
        b.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
