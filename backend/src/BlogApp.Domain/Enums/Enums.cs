namespace BlogApp.Domain.Enums;

// Enum THUẦN — không phụ thuộc Npgsql/EF Core (đúng nguyên tắc Clean Architecture: Domain
// không biết gì về hạ tầng cụ thể). Việc map sang native PostgreSQL ENUM TYPE (snake_case,
// đã tạo trong blog-database-schema.sql) được xử lý riêng ở Infrastructure layer bằng
// NpgsqlSnakeCaseNameTranslator — xem BlogApp.Infrastructure/Persistence/ApplicationDbContext.cs.

public enum PostStatus
{
    Published,
    Hidden
}

public enum MediaUsageType
{
    PostThumbnail,
    PostContent,
    HeroBanner,
    ProjectImage,
    OgImage,
    Other
}

public enum BannerType
{
    Banner,
    Slider
}

public enum AdPosition
{
    HomeMid,
    PostDetail
}

public enum ReviewStatus
{
    Visible,
    Hidden,
    Flagged
}

public enum SubscriberStatus
{
    Pending,
    Active,
    Unsubscribed
}

public enum OtpPurpose
{
    ReviewVerification,
    AdminPasswordReset
}

public enum CookieChoice
{
    Accepted,
    Declined
}
