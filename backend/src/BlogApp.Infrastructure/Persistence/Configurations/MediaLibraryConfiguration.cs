using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class MediaLibraryConfiguration : IEntityTypeConfiguration<MediaLibrary>
{
    public void Configure(EntityTypeBuilder<MediaLibrary> b)
    {
        b.ToTable("media_library");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.FileName).HasColumnName("file_name").IsRequired();
        b.Property(x => x.Url).HasColumnName("url").IsRequired();
        b.Property(x => x.Width).HasColumnName("width").IsRequired();
        b.Property(x => x.Height).HasColumnName("height").IsRequired();
        b.Property(x => x.FileSizeBytes).HasColumnName("file_size_bytes");
        b.Property(x => x.AltText).HasColumnName("alt_text");
        b.Property(x => x.UsageType).HasColumnName("usage_type").IsRequired();
        b.Property(x => x.UploadedAt).HasColumnName("uploaded_at").HasDefaultValueSql("now()");

        // Navigation ngược — chỉ để đọc, không cấu hình quan hệ ở đây (đã cấu hình phía Post/Banner/Project)
        b.Ignore(x => x.ThumbnailForPosts);
        b.Ignore(x => x.OgImageForPosts);
    }
}
