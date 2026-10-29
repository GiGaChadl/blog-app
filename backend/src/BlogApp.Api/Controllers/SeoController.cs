using BlogApp.Application.Features.System.Seo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
public class SeoController : ControllerBase
{
    private readonly SeoService _seoService;

    public SeoController(SeoService seoService)
    {
        _seoService = seoService;
    }

    /// <summary>Lấy cấu hình SEO mặc định toàn trang (công khai cho frontend).</summary>
    [HttpGet("api/seo")]
    [AllowAnonymous]
    public async Task<ActionResult<SeoSettingsDto>> GetSeoSettings(CancellationToken ct)
    {
        var result = await _seoService.GetSettingsAsync(ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên cập nhật cấu hình SEO toàn trang.</summary>
    [HttpPut("api/admin/seo")]
    [Authorize]
    public async Task<ActionResult<SeoSettingsDto>> UpdateSeoSettings([FromBody] UpdateSeoSettingsDto dto, CancellationToken ct)
    {
        var result = await _seoService.UpdateSettingsAsync(dto, ct);
        return Ok(result);
    }

    /// <summary>Cung cấp file robots.txt chuẩn cho công cụ tìm kiếm.</summary>
    [HttpGet("robots.txt")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRobotsTxt(CancellationToken ct)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var content = await _seoService.GetRobotsTxtContentAsync(baseUrl, ct);
        return Content(content, "text/plain; charset=utf-8");
    }

    /// <summary>Tự động sinh và trả về sitemap.xml chuẩn cho công cụ tìm kiếm Google/Bing.</summary>
    [HttpGet("sitemap.xml")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSitemapXml(CancellationToken ct)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var xml = await _seoService.GenerateSitemapXmlAsync(baseUrl, ct);
        return Content(xml, "application/xml; charset=utf-8");
    }
}
