using BlogApp.Domain.Enums;

namespace BlogApp.Application.Features.System.CookieConsent;

public record RecordConsentRequest(
    string VisitorToken,
    CookieChoice Choice
);

public record ConsentStatusDto(
    string VisitorToken,
    bool HasConsented,
    CookieChoice? Choice,
    DateTime? ConsentedAt
);
