using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class CookieConsentConfiguration : IEntityTypeConfiguration<CookieConsent>
{
    public void Configure(EntityTypeBuilder<CookieConsent> b)
    {
        b.ToTable("cookie_consents");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.VisitorToken).HasColumnName("visitor_token").IsRequired();
        b.Property(x => x.Choice).HasColumnName("choice").IsRequired();
        b.Property(x => x.ConsentedAt).HasColumnName("consented_at").HasDefaultValueSql("now()");

        b.HasIndex(x => x.VisitorToken);
    }
}
