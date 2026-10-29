using BlogApp.Application.Features.Interactions.Otp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController : ControllerBase
{
    private readonly OtpService _otpService;

    public OtpController(OtpService otpService)
    {
        _otpService = otpService;
    }

    /// <summary>
    /// Yêu cầu gửi mã OTP về email để xác thực trước khi gửi đánh giá bài viết.
    /// Giới hạn tối đa 5 lần gửi / email / 24h.
    /// </summary>
    [HttpPost("send-review-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendReviewOtp([FromBody] SendReviewOtpRequest request, CancellationToken ct)
    {
        var ipAddress = GetClientIpAddress();
        await _otpService.SendReviewOtpAsync(request, ipAddress, ct);
        return Ok(new { message = "Mã xác thực OTP đã được gửi đến email của bạn. Mã có hiệu lực trong 10 phút." });
    }

    /// <summary>
    /// Xác thực mã OTP và nhận Session ID có thời hạn 24 giờ.
    /// </summary>
    [HttpPost("verify-review-otp")]
    [AllowAnonymous]
    public async Task<ActionResult<ReviewSessionDto>> VerifyReviewOtp([FromBody] VerifyReviewOtpRequest request, CancellationToken ct)
    {
        var result = await _otpService.VerifyReviewOtpAsync(request, ct);
        return Ok(result);
    }

    private string? GetClientIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrWhiteSpace(ip))
                return ip;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }
}
