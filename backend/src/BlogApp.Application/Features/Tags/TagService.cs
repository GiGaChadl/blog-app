using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Common.Models;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Application.Features.Tags;

public class TagService
{
    private readonly IApplicationDbContext _db;

    public TagService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<TagDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _db.Tags.AsNoTracking();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TagDto(x.Id, x.Name, x.Slug, x.PostTags.Count))
            .ToListAsync(ct);

        return new PagedResult<TagDto>(items, total, page, pageSize);
    }

    public async Task<TagDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _db.Tags
            .Where(x => x.Slug == slug)
            .Select(x => new TagDto(x.Id, x.Name, x.Slug, x.PostTags.Count))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<TagDto> CreateAsync(CreateTagDto dto, CancellationToken ct = default)
    {
        var name = dto.Name.Trim();
        var slug = dto.Slug.Trim().ToLowerInvariant();

        ValidateRequiredFields(name, slug);

        if (await _db.Tags.AnyAsync(x => x.Name == name, ct))
            throw new ValidationException($"Tag \"{dto.Name}\" đã tồn tại.");

        if (await _db.Tags.AnyAsync(x => x.Slug == slug, ct))
            throw new ValidationException($"Slug \"{dto.Slug}\" đã tồn tại.");

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
        };

        _db.Tags.Add(tag);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (await _db.Tags.AnyAsync(x => x.Name == name, ct))
                throw new ValidationException($"Tag \"{dto.Name}\" đã tồn tại.");
            if (await _db.Tags.AnyAsync(x => x.Slug == slug, ct))
                throw new ValidationException($"Slug \"{dto.Slug}\" đã tồn tại.");
            throw new ValidationException("Tag hoặc Slug này đã tồn tại.");
        }

        return new TagDto(tag.Id, tag.Name, tag.Slug, 0);
    }

    public async Task UpdateAsync(Guid id, UpdateTagDto dto, CancellationToken ct = default)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Tag không tồn tại.");

        var name = dto.Name.Trim();
        var slug = dto.Slug.Trim().ToLowerInvariant();
        ValidateRequiredFields(name, slug);

        if (await _db.Tags.AnyAsync(x => x.Name == name && x.Id != id, ct))
            throw new ValidationException($"Tag \"{dto.Name}\" đã tồn tại.");

        if (await _db.Tags.AnyAsync(x => x.Slug == slug && x.Id != id, ct))
            throw new ValidationException($"Slug \"{dto.Slug}\" đã tồn tại.");

        tag.Name = name;
        tag.Slug = slug;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (await _db.Tags.AnyAsync(x => x.Name == name && x.Id != id, ct))
                throw new ValidationException($"Tag \"{dto.Name}\" đã tồn tại.");
            if (await _db.Tags.AnyAsync(x => x.Slug == slug && x.Id != id, ct))
                throw new ValidationException($"Slug \"{dto.Slug}\" đã tồn tại.");
            throw new ValidationException("Tag hoặc Slug này đã tồn tại.");
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Tag không tồn tại.");

        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidateRequiredFields(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Tên tag không được để trống.");

        if (string.IsNullOrWhiteSpace(slug))
            throw new ValidationException("Slug không được để trống.");
    }
}
