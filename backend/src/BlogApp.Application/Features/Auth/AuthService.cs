using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.Auth;

/// <summary>
/// Quản lý xác thực và tài khoản quản trị viên duy nhất của hệ thống.
/// Hỗ trợ đăng nhập JWT, đổi mật khẩu và khôi phục mật khẩu qua mã OTP email.
/// </summary>
public class AuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailSender _emailSender;

    public AuthService(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IEmailSender emailSender)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _emailSender = emailSender;
    }

    /// <returns>Null nếu sai username hoặc password — Controller tự quyết định trả 401 thế nào.</returns>
    public async Task<LoginResponseDto?> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var admin = await _db.AdminUsers.FirstOrDefaultAsync(x => x.Username == username, ct);
        if (admin is null) return null;
        if (!_passwordHasher.Verify(password, admin.PasswordHash)) return null;

        admin.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var token = _jwtTokenService.GenerateToken(admin);
        return new LoginResponseDto(token, admin.Username, admin.Email);
    }

    /// <summary>
    /// Đổi mật khẩu cho quản trị viên đang đăng nhập.
    /// </summary>
    public async Task ChangePasswordAsync(string username, ChangePasswordRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            throw new ValidationException("Mật khẩu mới phải có độ dài tối thiểu 6 ký tự.");

        var admin = await _db.AdminUsers.FirstOrDefaultAsync(x => x.Username == username, ct);
        if (admin is null)
            throw new ValidationException("Tài khoản quản trị viên không tồn tại.");

        if (!_passwordHasher.Verify(request.CurrentPassword, admin.PasswordHash))
            throw new ValidationException("Mật khẩu hiện tại không chính xác.");

        admin.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Yêu cầu gửi mã OTP về email quản trị viên để đặt lại mật khẩu khi quên.
    /// </summary>
    public async Task SendForgotPasswordOtpAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email không được để trống.");

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var admin = await _db.AdminUsers.FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail, ct);
        if (admin is null)
            throw new ValidationException("Email này không khớp với tài khoản quản trị viên hệ thống.");

        // Kiểm tra giới hạn gửi OTP 24h
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var count = await _db.OtpVerifications
            .CountAsync(x => x.Email == normalizedEmail &&
                             x.Purpose == OtpPurpose.AdminPasswordReset &&
                             x.CreatedAt >= cutoff, ct);

        if (count >= 5)
            throw new ValidationException("Bạn đã vượt quá số lần yêu cầu mã OTP cho phép trong 24 giờ. Vui lòng thử lại sau.");

        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var otpHash = HashOtp(otpCode);

        var otpEntity = new OtpVerification
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            OtpCodeHash = otpHash,
            Purpose = OtpPurpose.AdminPasswordReset,
            IpAddress = null, // Cho phép NULL khi purpose = AdminPasswordReset theo chk_otp_ip_required_for_review
            IsUsed = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            CreatedAt = DateTime.UtcNow
        };

        _db.OtpVerifications.Add(otpEntity);
        await _db.SaveChangesAsync(ct);

        var subject = "Khôi phục mật khẩu Quản trị viên";
        var bodyHtml = $@"
            <div style=""font-family: Arial, sans-serif; line-height: 1.6;"">
                <h2>Yêu cầu đặt lại mật khẩu</h2>
                <p>Hệ thống nhận được yêu cầu đặt lại mật khẩu cho tài khoản quản trị: <strong>{admin.Username}</strong>.</p>
                <p>Mã xác thực OTP của bạn là:</p>
                <div style=""font-size: 28px; font-weight: bold; color: #dc2626; letter-spacing: 4px; margin: 16px 0;"">
                    {otpCode}
                </div>
                <p>Mã này có hiệu lực trong vòng <strong>15 phút</strong>. Nếu bạn không thực hiện yêu cầu này, hãy bảo mật tài khoản ngay lập tức.</p>
            </div>";

        await _emailSender.SendEmailAsync(normalizedEmail, subject, bodyHtml, ct);
    }

    /// <summary>
    /// Đặt lại mật khẩu mới bằng mã OTP đã nhận qua email.
    /// </summary>
    public async Task ResetPasswordWithOtpAsync(ResetPasswordRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.OtpCode))
            throw new ValidationException("Email và mã OTP không được để trống.");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            throw new ValidationException("Mật khẩu mới phải có độ dài tối thiểu 6 ký tự.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var inputHash = HashOtp(request.OtpCode.Trim());
        var now = DateTime.UtcNow;

        var otpRecord = await _db.OtpVerifications
            .Where(x => x.Email == normalizedEmail &&
                        !x.IsUsed &&
                        x.Purpose == OtpPurpose.AdminPasswordReset &&
                        x.ExpiresAt > now)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otpRecord == null || otpRecord.OtpCodeHash != inputHash)
            throw new ValidationException("Mã OTP không chính xác hoặc đã hết hạn.");

        var admin = await _db.AdminUsers.FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail, ct);
        if (admin is null)
            throw new ValidationException("Tài khoản quản trị viên không tồn tại.");

        otpRecord.IsUsed = true;
        admin.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        await _db.SaveChangesAsync(ct);
    }

    private static string HashOtp(string otp)
    {
        var bytes = Encoding.UTF8.GetBytes(otp);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
