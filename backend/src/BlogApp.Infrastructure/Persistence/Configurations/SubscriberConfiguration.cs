using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class SubscriberConfiguration : IEntityTypeConfiguration<Subscriber>
{
    public void Configure(EntityTypeBuilder<Subscriber> b)
    {
        b.ToTable("subscribers");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Email).HasColumnName("email").IsRequired();
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.Status).HasColumnName("status").IsRequired();
        b.Property(x => x.ConfirmToken).HasColumnName("confirm_token");
        b.Property(x => x.ConfirmTokenExpiresAt).HasColumnName("confirm_token_expires_at");
        b.Property(x => x.ConfirmedAt).HasColumnName("confirmed_at");
        b.Property(x => x.UnsubscribedAt).HasColumnName("unsubscribed_at");
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

        b.HasIndex(x => x.Status);
    }
}
