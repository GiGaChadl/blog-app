using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.Administration.Slogans;

public class SloganService
{
    private readonly IApplicationDbContext _db;

    public SloganService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy slogan đang hiển thị duy nhất của blog (công khai).
    /// </summary>
    public async Task<SloganDto?> GetActiveSloganAsync(CancellationToken ct = default)
    {
        var slogan = await _db.Slogans
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IsActive, ct);

        if (slogan == null) return null;

        return new SloganDto(slogan.Id, slogan.Content, slogan.IsActive, slogan.CreatedAt);
    }

    /// <summary>
    /// Quản trị viên xem toàn bộ danh sách slogan.
    /// </summary>
    public async Task<List<SloganDto>> GetAllSlogansAsync(CancellationToken ct = default)
    {
        return await _db.Slogans
            .AsNoTracking()
            .OrderByDescending(s => s.IsActive)
            .ThenByDescending(s => s.CreatedAt)
            .Select(s => new SloganDto(s.Id, s.Content, s.IsActive, s.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<SloganDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var slogan = await _db.Slogans
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (slogan == null)
            throw new ValidationException("Slogan không tồn tại.");

        return new SloganDto(slogan.Id, slogan.Content, slogan.IsActive, slogan.CreatedAt);
    }

    /// <summary>
    /// Tạo slogan mới. Nếu IsActive = true, tự động tắt slogan đang active cũ để thỏa mãn partial unique index.
    /// </summary>
    public async Task<SloganDto> CreateAsync(CreateSloganDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new ValidationException("Nội dung slogan không được để trống.");

        if (dto.IsActive)
        {
            var activeSlogans = await _db.Slogans.Where(s => s.IsActive).ToListAsync(ct);
            foreach (var old in activeSlogans)
            {
                old.IsActive = false;
            }
        }

        var slogan = new Slogan
        {
            Id = Guid.NewGuid(),
            Content = dto.Content.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Slogans.Add(slogan);
        await _db.SaveChangesAsync(ct);

        return new SloganDto(slogan.Id, slogan.Content, slogan.IsActive, slogan.CreatedAt);
    }

    /// <summary>
    /// Cập nhật slogan. Nếu kích hoạt IsActive = true, tự động tắt slogan đang active cũ.
    /// </summary>
    public async Task<SloganDto> UpdateAsync(Guid id, UpdateSloganDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new ValidationException("Nội dung slogan không được để trống.");

        var slogan = await _db.Slogans.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (slogan == null)
            throw new ValidationException("Slogan không tồn tại.");

        if (dto.IsActive && !slogan.IsActive)
        {
            var otherActives = await _db.Slogans.Where(s => s.IsActive && s.Id != id).ToListAsync(ct);
            foreach (var old in otherActives)
            {
                old.IsActive = false;
            }
        }

        slogan.Content = dto.Content.Trim();
        slogan.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(ct);

        return new SloganDto(slogan.Id, slogan.Content, slogan.IsActive, slogan.CreatedAt);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var slogan = await _db.Slogans.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (slogan == null)
            throw new ValidationException("Slogan không tồn tại.");

        _db.Slogans.Remove(slogan);
        await _db.SaveChangesAsync(ct);
    }
}
