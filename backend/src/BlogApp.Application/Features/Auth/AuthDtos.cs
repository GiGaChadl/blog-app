namespace BlogApp.Application.Features.Auth;

public record LoginRequestDto(string Username, string Password);

public record LoginResponseDto(string Token, string Username, string Email);

public record ChangePasswordRequestDto(string CurrentPassword, string NewPassword);

public record ForgotPasswordRequestDto(string Email);

public record ResetPasswordRequestDto(string Email, string OtpCode, string NewPassword);
