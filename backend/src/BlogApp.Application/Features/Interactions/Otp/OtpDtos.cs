namespace BlogApp.Application.Features.Interactions.Otp;

public record SendReviewOtpRequest(string Email);

public record VerifyReviewOtpRequest(string Email, string OtpCode);

public record ReviewSessionDto(Guid SessionId, DateTime ExpiresAt, string Email);
