using BlogApp.Application.Common.Models;
using BlogApp.Application.Features.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly PostService _postService;
    public PostsController(PostService postService) => _postService = postService;

    [AllowAnonymous, HttpGet]
    public async Task<ActionResult<PagedResult<PostSummaryDto>>> GetPaged([FromQuery] PostFilterDto filter, CancellationToken ct) => Ok(await _postService.GetPagedAsync(filter, ct: ct));
    [AllowAnonymous, HttpGet("featured")]
    public async Task<ActionResult<List<PostSummaryDto>>> GetFeatured([FromQuery] int count = 5, CancellationToken ct = default) => Ok(await _postService.GetFeaturedAsync(count, ct));
    [Authorize, HttpGet("stats")]
    public async Task<ActionResult> GetStats(CancellationToken ct) => Ok(new { totalPosts = await _postService.GetTotalCountAsync(ct), totalViews = await _postService.GetTotalViewsAsync(ct) });
    [AllowAnonymous, HttpGet("{id:guid}")]
    public async Task<ActionResult<PostDetailDto>> GetById(Guid id, CancellationToken ct = default) { var post = await _postService.GetByIdAsync(id, ct); return post is null ? NotFound() : Ok(post); }
    [AllowAnonymous, HttpGet("{slug}")]
    public async Task<ActionResult<PostDetailDto>> GetBySlug(string slug, [FromQuery] bool incrementView = false, CancellationToken ct = default) { var post = await _postService.GetBySlugAsync(slug, incrementView, ct); return post is null ? NotFound() : Ok(post); }
    [AllowAnonymous, HttpGet("{id:guid}/related")]
    public async Task<ActionResult<List<PostSummaryDto>>> GetRelated(Guid id, [FromQuery] int count = 4, CancellationToken ct = default) => Ok(await _postService.GetRelatedAsync(id, count, ct));
    [Authorize, HttpPost]
    public async Task<ActionResult<PostDetailDto>> Create(CreatePostDto dto, CancellationToken ct) => Ok(await _postService.CreateAsync(dto, ct));
    [Authorize, HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePostDto dto, CancellationToken ct) { await _postService.UpdateAsync(id, dto, ct); return NoContent(); }
    [Authorize, HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await _postService.DeleteAsync(id, ct); return NoContent(); }
}
