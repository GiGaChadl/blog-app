using BlogApp.Application.Features.Administration.Ads;
using BlogApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
public class AdsController : ControllerBase
{
    private readonly AdService _adService;

    public AdsController(AdService adService)
    {
        _adService = adService;
    }

    /// <summary>Lấy cấu hình Google AdSense đang hoạt động theo vị trí (công khai cho người đọc).</summary>
    [HttpGet("api/ads/{position}")]
    [AllowAnonymous]
    public async Task<ActionResult<AdDto>> GetActiveAd(AdPosition position, CancellationToken ct)
    {
        var result = await _adService.GetActiveAdByPositionAsync(position, ct);
        if (result == null)
            return NoContent();

        return Ok(result);
    }

    /// <summary>Quản trị viên xem toàn bộ cấu hình quảng cáo.</summary>
    [HttpGet("api/admin/ads")]
    [Authorize]
    public async Task<ActionResult<List<AdDto>>> GetAllAds(CancellationToken ct)
    {
        var result = await _adService.GetAllAdsAsync(ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên cập nhật hoặc tạo mới cấu hình quảng cáo cho một vị trí (Upsert).</summary>
    [HttpPut("api/admin/ads/{position}")]
    [Authorize]
    public async Task<ActionResult<AdDto>> UpsertAd(AdPosition position, [FromBody] UpsertAdDto dto, CancellationToken ct)
    {
        var result = await _adService.UpsertAdAsync(position, dto, ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên xóa cấu hình quảng cáo theo vị trí.</summary>
    [HttpDelete("api/admin/ads/{position}")]
    [Authorize]
    public async Task<IActionResult> DeleteAd(AdPosition position, CancellationToken ct)
    {
        await _adService.DeleteAdAsync(position, ct);
        return NoContent();
    }
}
