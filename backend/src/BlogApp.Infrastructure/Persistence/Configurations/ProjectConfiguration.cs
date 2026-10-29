using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.ToTable("projects");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.MediaId).HasColumnName("media_id").IsRequired();
        b.Property(x => x.Title).HasColumnName("title").IsRequired();
        b.Property(x => x.ShortDescription).HasColumnName("short_description");
        b.Property(x => x.Tag).HasColumnName("tag");
        b.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);

        b.HasOne(x => x.Media)
            .WithMany(x => x.Projects)
            .HasForeignKey(x => x.MediaId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.MediaId);
    }
}
