using BlogApp.Application.Features.System.CookieConsent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
[Route("api/cookie-consent")]
public class CookieConsentController : ControllerBase
{
    private readonly CookieConsentService _cookieService;

    public CookieConsentController(CookieConsentService cookieService)
    {
        _cookieService = cookieService;
    }

    /// <summary>Ghi nhận lựa chọn Cookie của độc giả (Đồng ý hoặc Thu hồi).</summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ConsentStatusDto>> RecordConsent([FromBody] RecordConsentRequest request, CancellationToken ct)
    {
        var result = await _cookieService.RecordConsentAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Kiểm tra trạng thái đồng ý Cookie của độc giả.</summary>
    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<ActionResult<ConsentStatusDto>> GetStatus([FromQuery] string visitorToken, CancellationToken ct)
    {
        var result = await _cookieService.GetStatusAsync(visitorToken, ct);
        return Ok(result);
    }
}
