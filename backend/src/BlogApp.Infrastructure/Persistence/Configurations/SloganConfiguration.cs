using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class SloganConfiguration : IEntityTypeConfiguration<Slogan>
{
    public void Configure(EntityTypeBuilder<Slogan> b)
    {
        b.ToTable("slogans");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Content).HasColumnName("content").IsRequired();
        b.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(false);
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

        // Partial unique index — chỉ tối đa 1 dòng is_active = true. EF Core 7+ hỗ trợ điều
        // kiện lọc qua HasFilter(). Đổi slogan đang hiện PHẢI làm trong 1 transaction ở
        // Application (tắt dòng cũ trước, bật dòng mới sau) — xem ApplicationDbContext ghi chú.
        b.HasIndex(x => x.IsActive)
            .IsUnique()
            .HasFilter("is_active = true")
            .HasDatabaseName("uq_slogans_single_active");
    }
}
