using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class VisitorLogConfiguration : IEntityTypeConfiguration<VisitorLog>
{
    public void Configure(EntityTypeBuilder<VisitorLog> b)
    {
        b.ToTable("visitor_logs");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.IpAddress).HasColumnName("ip_address");
        b.Property(x => x.UserAgent).HasColumnName("user_agent");
        b.Property(x => x.DeviceType).HasColumnName("device_type");
        b.Property(x => x.Country).HasColumnName("country");
        b.Property(x => x.City).HasColumnName("city");
        b.Property(x => x.PageUrl).HasColumnName("page_url");
        b.Property(x => x.Referrer).HasColumnName("referrer");
        b.Property(x => x.VisitedAt).HasColumnName("visited_at").HasDefaultValueSql("now()");

        b.HasIndex(x => x.VisitedAt);
    }
}
