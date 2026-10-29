using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> b)
    {
        // ip_address CHỈ bắt buộc khi purpose = review_verification (xem CHECK bên dưới) —
        // khớp đúng chuỗi PgName do NpgsqlSnakeCaseNameTranslator sinh ra cho OtpPurpose.ReviewVerification.
        b.ToTable("otp_verifications", tb => tb.HasCheckConstraint(
            "chk_otp_ip_required_for_review",
            "(purpose = 'review_verification' AND ip_address IS NOT NULL) OR (purpose = 'admin_password_reset')"));
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Email).HasColumnName("email").IsRequired();
        b.Property(x => x.OtpCodeHash).HasColumnName("otp_code_hash").IsRequired();
        b.Property(x => x.Purpose).HasColumnName("purpose").IsRequired();
        b.Property(x => x.IpAddress).HasColumnName("ip_address");
        b.Property(x => x.IsUsed).HasColumnName("is_used").HasDefaultValue(false);
        b.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

        b.HasIndex(x => new { x.Email, x.CreatedAt });
        b.HasIndex(x => new { x.IpAddress, x.CreatedAt });
    }
}
