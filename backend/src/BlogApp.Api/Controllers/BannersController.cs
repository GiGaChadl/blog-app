using BlogApp.Application.Features.Administration.Banners;
using BlogApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
public class BannersController : ControllerBase
{
    private readonly BannerService _bannerService;

    public BannersController(BannerService bannerService)
    {
        _bannerService = bannerService;
    }

    /// <summary>Lấy danh sách banner/slider đang hoạt động hiển thị công khai trên trang chủ.</summary>
    [HttpGet("api/banners")]
    [AllowAnonymous]
    public async Task<ActionResult<List<BannerDto>>> GetActiveBanners([FromQuery] BannerType? type, CancellationToken ct)
    {
        var result = await _bannerService.GetActiveBannersAsync(type, ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên xem danh sách toàn bộ banner/slider.</summary>
    [HttpGet("api/admin/banners")]
    [Authorize]
    public async Task<ActionResult<List<BannerDto>>> GetAllBanners([FromQuery] BannerType? type, CancellationToken ct)
    {
        var result = await _bannerService.GetAllBannersAsync(type, ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên lấy chi tiết banner.</summary>
    [HttpGet("api/admin/banners/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<BannerDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _bannerService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên tạo banner mới.</summary>
    [HttpPost("api/admin/banners")]
    [Authorize]
    public async Task<ActionResult<BannerDto>> Create([FromBody] CreateBannerDto dto, CancellationToken ct)
    {
        var result = await _bannerService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Quản trị viên cập nhật banner.</summary>
    [HttpPut("api/admin/banners/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<BannerDto>> Update(Guid id, [FromBody] UpdateBannerDto dto, CancellationToken ct)
    {
        var result = await _bannerService.UpdateAsync(id, dto, ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên xóa banner.</summary>
    [HttpDelete("api/admin/banners/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _bannerService.DeleteAsync(id, ct);
        return NoContent();
    }
}
