using BlogApp.Application.Features.Administration.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>Lấy danh sách các dự án tiêu biểu (hỗ trợ cả route công khai và route quản trị).</summary>
    [HttpGet("api/projects")]
    [HttpGet("api/admin/projects")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ProjectDto>>> GetProjects(CancellationToken ct)
    {
        var result = await _projectService.GetProjectsAsync(ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên lấy chi tiết dự án.</summary>
    [HttpGet("api/admin/projects/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _projectService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên tạo dự án mới.</summary>
    [HttpPost("api/admin/projects")]
    [Authorize]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectDto dto, CancellationToken ct)
    {
        var result = await _projectService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Quản trị viên cập nhật dự án.</summary>
    [HttpPut("api/admin/projects/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, [FromBody] UpdateProjectDto dto, CancellationToken ct)
    {
        var result = await _projectService.UpdateAsync(id, dto, ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên xóa dự án.</summary>
    [HttpDelete("api/admin/projects/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _projectService.DeleteAsync(id, ct);
        return NoContent();
    }
}
