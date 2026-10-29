using BlogApp.Application.Features.Auth;
using BlogApp.Application.Features.Categories;
using BlogApp.Application.Features.Tags;
using BlogApp.Application.Features.Posts;
using BlogApp.Application.Features.Media;
using BlogApp.Application.Features.Interactions.Otp;
using BlogApp.Application.Features.Interactions.Reviews;
using BlogApp.Application.Features.Interactions.Subscribers;
using BlogApp.Application.Features.Administration.Banners;
using BlogApp.Application.Features.Administration.Slogans;
using BlogApp.Application.Features.Administration.Ads;
using BlogApp.Application.Features.Administration.Projects;
using BlogApp.Application.Features.Administration.Contact;
using BlogApp.Application.Features.System.CookieConsent;
using BlogApp.Application.Features.System.Analytics;
using BlogApp.Application.Features.System.Smtp;
using BlogApp.Application.Features.System.Seo;
using BlogApp.Application.Features.System.Maintenance;
using Microsoft.Extensions.DependencyInjection;

namespace BlogApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<TagService>();
        services.AddScoped<PostService>();
        services.AddScoped<MediaService>();
        services.AddScoped<OtpService>();
        services.AddScoped<PostReviewService>();
        services.AddScoped<SubscriberService>();
        services.AddScoped<BannerService>();
        services.AddScoped<SloganService>();
        services.AddScoped<AdService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<ContactInfoService>();
        services.AddScoped<CookieConsentService>();
        services.AddScoped<VisitorTrackingService>();
        services.AddScoped<SmtpSettingsService>();
        services.AddScoped<SeoService>();
        services.AddScoped<SystemMaintenanceService>();
        return services;
    }
}
