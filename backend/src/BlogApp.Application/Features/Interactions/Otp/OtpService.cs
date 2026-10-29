using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.Interactions.Otp;

public class OtpService
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailSender _emailSender;
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public OtpService(IApplicationDbContext db, IEmailSender emailSender)
    {
        _db = db;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Gửi mã OTP phục vụ xác thực người dùng trước khi đánh giá bài viết.
    /// Giới hạn tối đa 5 lần gửi / email / 24 giờ để chống lạm dụng và spam.
    /// Bắt buộc ghi nhận IP address để thỏa mãn CHECK constraint DB chk_otp_ip_required_for_review.
    /// </summary>
    public async Task SendReviewOtpAsync(SendReviewOtpRequest request, string? ipAddress, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !EmailRegex.IsMatch(request.Email.Trim()))
            throw new ValidationException("Địa chỉ email không hợp lệ.");

        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ValidationException("Không thể xác thực địa chỉ IP của bạn. Vui lòng thử lại.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var clientIp = ipAddress.Trim();

        // 1. Kiểm tra Rate-limiting: Tối đa 5 lần gửi OTP cho cùng 1 email trong 24 giờ qua
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        var recentCount = await _db.OtpVerifications
            .CountAsync(x => x.Email == normalizedEmail && x.CreatedAt >= cutoffTime, ct);

        if (recentCount >= 5)
            throw new ValidationException("Bạn đã gửi mã OTP quá số lần cho phép (tối đa 5 lần trong 24 giờ). Vui lòng thử lại sau.");

        // 2. Sinh mã ngẫu nhiên 6 chữ số và mã hóa SHA-256 để lưu DB
        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var otpHash = HashOtp(otpCode);

        var otpEntity = new OtpVerification
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            OtpCodeHash = otpHash,
            Purpose = OtpPurpose.ReviewVerification,
            IpAddress = clientIp,
            IsUsed = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };

        _db.OtpVerifications.Add(otpEntity);
        await _db.SaveChangesAsync(ct);

        // 3. Gửi email chứa mã OTP
        var subject = "Mã xác thực đánh giá bài viết";
        var bodyHtml = $@"
            <div style=""font-family: Arial, sans-serif; line-height: 1.6;"">
                <h2>Xác thực đánh giá bài viết</h2>
                <p>Bạn nhận được email này vì đã yêu cầu gửi đánh giá hoặc bình luận bài viết.</p>
                <p>Mã xác thực OTP của bạn là:</p>
                <div style=""font-size: 28px; font-weight: bold; color: #2563eb; letter-spacing: 4px; margin: 16px 0;"">
                    {otpCode}
                </div>
                <p>Mã này có hiệu lực trong vòng <strong>10 phút</strong>. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
            </div>";

        await _emailSender.SendEmailAsync(normalizedEmail, subject, bodyHtml, ct);
    }

    /// <summary>
    /// Xác thực mã OTP người dùng nhập vào.
    /// Nếu đúng, tạo một ReviewSession sống 24 giờ để người dùng có thể gửi/sửa đánh giá trong phiên.
    /// </summary>
    public async Task<ReviewSessionDto> VerifyReviewOtpAsync(VerifyReviewOtpRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.OtpCode))
            throw new ValidationException("Email và mã OTP không được để trống.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var inputHash = HashOtp(request.OtpCode.Trim());
        var now = DateTime.UtcNow;

        // Tìm mã OTP chưa sử dụng, đúng mục đích, còn hạn
        var otpRecord = await _db.OtpVerifications
            .Where(x => x.Email == normalizedEmail &&
                        !x.IsUsed &&
                        x.Purpose == OtpPurpose.ReviewVerification &&
                        x.ExpiresAt > now)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otpRecord == null || otpRecord.OtpCodeHash != inputHash)
            throw new ValidationException("Mã OTP không chính xác hoặc đã hết hạn.");

        // Đánh dấu OTP đã sử dụng
        otpRecord.IsUsed = true;

        // Tạo ReviewSession có hạn 24 giờ
        var session = new ReviewSession
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            CreatedAt = now,
            ExpiresAt = now.AddHours(24)
        };

        _db.ReviewSessions.Add(session);
        await _db.SaveChangesAsync(ct);

        return new ReviewSessionDto(session.Id, session.ExpiresAt, session.Email);
    }

    private static string HashOtp(string otp)
    {
        var bytes = Encoding.UTF8.GetBytes(otp);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
