using BlogApp.Application.Common.Models;
using BlogApp.Application.Features.Media;
using BlogApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
[Route("api/media")]
public class MediaLibraryController : ControllerBase
{
    private readonly MediaService _mediaService;

    public MediaLibraryController(MediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResult<MediaDto>>> GetPaged([FromQuery] MediaFilterDto filter, CancellationToken ct)
    {
        return Ok(await _mediaService.GetPagedAsync(filter, ct));
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MediaDto>> GetById(Guid id, CancellationToken ct)
    {
        var media = await _mediaService.GetByIdAsync(id, ct);
        return media is null ? NotFound() : Ok(media);
    }

    [Authorize]
    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<MediaDto>> Upload(
        [FromForm] IFormFile file,
        [FromForm] string? altText,
        [FromForm] MediaUsageType usageType = MediaUsageType.PostThumbnail,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn file để tải lên." });
        }

        await using var stream = file.OpenReadStream();
        var request = new UploadMediaRequest(
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            altText,
            usageType
        );

        var result = await _mediaService.UploadAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMediaDto dto, CancellationToken ct)
    {
        await _mediaService.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediaService.DeleteAsync(id, ct);
        return NoContent();
    }
}
