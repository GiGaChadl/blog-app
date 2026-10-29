using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> b)
    {
        b.ToTable("tags");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Name).HasColumnName("name").IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
        b.Property(x => x.Slug).HasColumnName("slug").IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
    }
}
