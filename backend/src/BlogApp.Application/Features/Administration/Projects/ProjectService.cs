using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.Administration.Projects;

public class ProjectService
{
    private readonly IApplicationDbContext _db;

    public ProjectService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ dự án tiêu biểu sắp xếp theo thứ tự hiển thị (công khai).
    /// </summary>
    public async Task<List<ProjectDto>> GetProjectsAsync(CancellationToken ct = default)
    {
        return await _db.Projects
            .AsNoTracking()
            .Include(p => p.Media)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Title)
            .Select(p => new ProjectDto(
                p.Id,
                p.MediaId,
                p.Media.Url,
                p.Media.AltText,
                p.Title,
                p.ShortDescription,
                p.Tag,
                p.SortOrder
            ))
            .ToListAsync(ct);
    }

    public async Task<ProjectDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (project == null)
            throw new ValidationException("Dự án không tồn tại.");

        return new ProjectDto(
            project.Id,
            project.MediaId,
            project.Media.Url,
            project.Media.AltText,
            project.Title,
            project.ShortDescription,
            project.Tag,
            project.SortOrder
        );
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto, CancellationToken ct = default)
    {
        if (dto.SortOrder < 1)
            throw new ValidationException("Thứ tự sắp xếp phải lớn hơn hoặc bằng 1.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ValidationException("Tiêu đề dự án không được để trống.");

        var mediaExists = await _db.MediaLibrary.AnyAsync(m => m.Id == dto.MediaId, ct);
        if (!mediaExists)
            throw new ValidationException("Ảnh liên kết không tồn tại trong thư viện media.");

        var project = new Project
        {
            Id = Guid.NewGuid(),
            MediaId = dto.MediaId,
            Title = dto.Title.Trim(),
            ShortDescription = string.IsNullOrWhiteSpace(dto.ShortDescription) ? null : dto.ShortDescription.Trim(),
            Tag = string.IsNullOrWhiteSpace(dto.Tag) ? null : dto.Tag.Trim(),
            SortOrder = dto.SortOrder
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(project.Id, ct);
    }

    public async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto dto, CancellationToken ct = default)
    {
        if (dto.SortOrder < 1)
            throw new ValidationException("Thứ tự sắp xếp phải lớn hơn hoặc bằng 1.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ValidationException("Tiêu đề dự án không được để trống.");

        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project == null)
            throw new ValidationException("Dự án không tồn tại.");

        var mediaExists = await _db.MediaLibrary.AnyAsync(m => m.Id == dto.MediaId, ct);
        if (!mediaExists)
            throw new ValidationException("Ảnh liên kết không tồn tại trong thư viện media.");

        project.MediaId = dto.MediaId;
        project.Title = dto.Title.Trim();
        project.ShortDescription = string.IsNullOrWhiteSpace(dto.ShortDescription) ? null : dto.ShortDescription.Trim();
        project.Tag = string.IsNullOrWhiteSpace(dto.Tag) ? null : dto.Tag.Trim();
        project.SortOrder = dto.SortOrder;

        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(project.Id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project == null)
            throw new ValidationException("Dự án không tồn tại.");

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);
    }
}
