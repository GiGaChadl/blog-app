using BlogApp.Application.Common.Interfaces;
using BlogApp.Infrastructure.Identity;
using BlogApp.Infrastructure.Persistence;
using BlogApp.Infrastructure.Services;
using BlogApp.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BlogApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection trong appsettings.json");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<Domain.Enums.PostStatus>("post_status");
                npgsqlOptions.MapEnum<Domain.Enums.MediaUsageType>("media_usage_type");
                npgsqlOptions.MapEnum<Domain.Enums.BannerType>("banner_type");
                npgsqlOptions.MapEnum<Domain.Enums.AdPosition>("ad_position");
                npgsqlOptions.MapEnum<Domain.Enums.ReviewStatus>("review_status");
                npgsqlOptions.MapEnum<Domain.Enums.SubscriberStatus>("subscriber_status");
                npgsqlOptions.MapEnum<Domain.Enums.OtpPurpose>("otp_purpose");
                npgsqlOptions.MapEnum<Domain.Enums.CookieChoice>("cookie_choice");
            }));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IFileStorageService, LocalStorageService>();
        services.AddScoped<IEmailSender, MailKitEmailSender>();
        services.AddSingleton<IEncryptionService>(provider => new AesEncryptionService(provider.GetRequiredService<IConfiguration>()));
        services.AddSingleton<AesEncryptionService>(provider => new AesEncryptionService(provider.GetRequiredService<IConfiguration>()));

        return services;
    }
}
