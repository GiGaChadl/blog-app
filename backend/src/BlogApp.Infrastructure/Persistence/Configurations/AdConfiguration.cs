using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class AdConfiguration : IEntityTypeConfiguration<Ad>
{
    public void Configure(EntityTypeBuilder<Ad> b)
    {
        b.ToTable("ads");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Position).HasColumnName("position").IsRequired();
        b.HasIndex(x => x.Position).IsUnique();
        b.Property(x => x.AdsenseSlotId).HasColumnName("adsense_slot_id").IsRequired();
        b.Property(x => x.StartDate).HasColumnName("start_date");
        b.Property(x => x.EndDate).HasColumnName("end_date");
        b.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
    }
}
