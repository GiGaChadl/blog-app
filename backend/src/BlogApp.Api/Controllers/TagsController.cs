using BlogApp.Application.Common.Models;
using BlogApp.Application.Features.Tags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController : ControllerBase
{
    private readonly TagService _tagService;

    public TagsController(TagService tagService)
    {
        _tagService = tagService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResult<TagDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        return Ok(await _tagService.GetAllAsync(page, pageSize, ct));
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<TagDto>> GetBySlug(string slug, CancellationToken ct)
    {
        var tag = await _tagService.GetBySlugAsync(slug, ct);
        return tag is null ? NotFound() : Ok(tag);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<TagDto>> Create(CreateTagDto dto, CancellationToken ct)
    {
        return Ok(await _tagService.CreateAsync(dto, ct));
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTagDto dto, CancellationToken ct)
    {
        await _tagService.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _tagService.DeleteAsync(id, ct);
        return NoContent();
    }
}
