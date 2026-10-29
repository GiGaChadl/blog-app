using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Persistence.Configurations;

public class SmtpSettingsConfiguration : IEntityTypeConfiguration<SmtpSettings>
{
    public void Configure(EntityTypeBuilder<SmtpSettings> b)
    {
        b.ToTable("smtp_settings");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Host).HasColumnName("host");
        b.Property(x => x.Port).HasColumnName("port");
        b.Property(x => x.Username).HasColumnName("username");
        // Mã hoá 2 chiều — KHÔNG phải hash. Khoá mã hoá nằm ở biến môi trường, không trong DB.
        b.Property(x => x.PasswordEncrypted).HasColumnName("password_encrypted");
        b.Property(x => x.SenderEmail).HasColumnName("sender_email");
        b.Property(x => x.SenderName).HasColumnName("sender_name");
        b.Property(x => x.DailyLimit).HasColumnName("daily_limit").HasDefaultValue(50);
        b.Property(x => x.EmailsSentToday).HasColumnName("emails_sent_today").HasDefaultValue(0);
        b.Property(x => x.LastResetAt).HasColumnName("last_reset_at");
    }
}
