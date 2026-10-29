using BlogApp.Application.Features.Administration.Slogans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
public class SlogansController : ControllerBase
{
    private readonly SloganService _sloganService;

    public SlogansController(SloganService sloganService)
    {
        _sloganService = sloganService;
    }

    /// <summary>Lấy slogan đang hoạt động duy nhất của blog (công khai).</summary>
    [HttpGet("api/slogans/active")]
    [AllowAnonymous]
    public async Task<ActionResult<SloganDto>> GetActiveSlogan(CancellationToken ct)
    {
        var result = await _sloganService.GetActiveSloganAsync(ct);
        if (result == null)
            return NoContent();

        return Ok(result);
    }

    /// <summary>Quản trị viên xem toàn bộ danh sách slogan.</summary>
    [HttpGet("api/admin/slogans")]
    [Authorize]
    public async Task<ActionResult<List<SloganDto>>> GetAllSlogans(CancellationToken ct)
    {
        var result = await _sloganService.GetAllSlogansAsync(ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên lấy chi tiết một slogan.</summary>
    [HttpGet("api/admin/slogans/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<SloganDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sloganService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên tạo slogan mới.</summary>
    [HttpPost("api/admin/slogans")]
    [Authorize]
    public async Task<ActionResult<SloganDto>> Create([FromBody] CreateSloganDto dto, CancellationToken ct)
    {
        var result = await _sloganService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Quản trị viên cập nhật slogan.</summary>
    [HttpPut("api/admin/slogans/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<SloganDto>> Update(Guid id, [FromBody] UpdateSloganDto dto, CancellationToken ct)
    {
        var result = await _sloganService.UpdateAsync(id, dto, ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên xóa slogan.</summary>
    [HttpDelete("api/admin/slogans/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sloganService.DeleteAsync(id, ct);
        return NoContent();
    }
}
