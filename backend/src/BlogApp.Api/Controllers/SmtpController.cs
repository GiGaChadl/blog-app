using BlogApp.Application.Features.System.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
[Route("api/admin/smtp")]
[Authorize]
public class SmtpController : ControllerBase
{
    private readonly SmtpSettingsService _smtpService;

    public SmtpController(SmtpSettingsService smtpService)
    {
        _smtpService = smtpService;
    }

    /// <summary>Quản trị viên xem cấu hình máy chủ SMTP (ẩn mật khẩu thật).</summary>
    [HttpGet]
    public async Task<ActionResult<SmtpSettingsDto>> GetSettings(CancellationToken ct)
    {
        var result = await _smtpService.GetSettingsAsync(ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên cập nhật cấu hình máy chủ SMTP (mật khẩu được mã hoá 2 chiều AES-256).</summary>
    [HttpPut]
    public async Task<ActionResult<SmtpSettingsDto>> UpdateSettings([FromBody] UpdateSmtpSettingsDto dto, CancellationToken ct)
    {
        var result = await _smtpService.UpdateSettingsAsync(dto, ct);
        return Ok(result);
    }

    /// <summary>Gửi email thử nghiệm để kiểm tra cấu hình SMTP.</summary>
    [HttpPost("test")]
    public async Task<IActionResult> TestConnection([FromBody] TestSmtpRequest request, CancellationToken ct)
    {
        await _smtpService.TestSmtpConnectionAsync(request, ct);
        return Ok(new { message = $"Đã gửi email kiểm tra thành công tới {request.ToEmail}." });
    }
}
