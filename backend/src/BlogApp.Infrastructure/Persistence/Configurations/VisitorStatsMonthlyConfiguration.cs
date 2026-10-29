using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class VisitorStatsMonthlyConfiguration : IEntityTypeConfiguration<VisitorStatsMonthly>
{
    public void Configure(EntityTypeBuilder<VisitorStatsMonthly> b)
    {
        b.ToTable("visitor_stats_monthly");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        // Quy ước: Month luôn là ngày 01 của tháng (VD 2026-08-01 = "tháng 8/2026").
        b.Property(x => x.Month).HasColumnName("month").IsRequired();
        b.HasIndex(x => x.Month).IsUnique();
        b.Property(x => x.TotalVisits).HasColumnName("total_visits").IsRequired();
        b.Property(x => x.UniqueVisitors).HasColumnName("unique_visitors").IsRequired();
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
    }
}
