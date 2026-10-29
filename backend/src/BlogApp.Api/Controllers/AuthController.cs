using System.Security.Claims;
using BlogApp.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Đăng nhập admin duy nhất của hệ thống. Trả JWT dùng cho mọi endpoint [Authorize] sau này.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request.Username, request.Password, ct);
        if (result is null)
            return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

        return Ok(result);
    }

    /// <summary>Đổi mật khẩu cho quản trị viên đang đăng nhập.</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken ct)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) 
            ?? User.FindFirstValue("sub")
            ?? User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(username))
            return Unauthorized(new { message = "Không xác định được danh tính người dùng." });

        await _authService.ChangePasswordAsync(username, request, ct);
        return Ok(new { message = "Đổi mật khẩu thành công." });
    }

    /// <summary>Yêu cầu gửi mã OTP để đặt lại mật khẩu khi quên.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken ct)
    {
        await _authService.SendForgotPasswordOtpAsync(request.Email, ct);
        return Ok(new { message = "Mã xác thực OTP đã được gửi về email quản trị viên. Vui lòng kiểm tra hộp thư." });
    }

    /// <summary>Đặt lại mật khẩu mới bằng mã OTP.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken ct)
    {
        await _authService.ResetPasswordWithOtpAsync(request, ct);
        return Ok(new { message = "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới." });
    }
}
