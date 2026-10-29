using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.Administration.Banners;

public class BannerService
{
    private readonly IApplicationDbContext _db;

    public BannerService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy danh sách banner/slider đang hiển thị cho trang chủ (công khai).
    /// </summary>
    public async Task<List<BannerDto>> GetActiveBannersAsync(BannerType? type = null, CancellationToken ct = default)
    {
        var query = _db.Banners
            .AsNoTracking()
            .Include(b => b.Media)
            .Where(b => b.IsActive);

        if (type.HasValue)
            query = query.Where(b => b.Type == type.Value);

        return await query
            .OrderBy(b => b.SortOrder)
            .ThenBy(b => b.CreatedAt)
            .Select(b => new BannerDto(
                b.Id,
                b.MediaId,
                b.Media.Url,
                b.Media.AltText,
                b.Type,
                b.LinkUrl,
                b.SortOrder,
                b.IsActive,
                b.CreatedAt
            ))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Quản trị viên xem danh sách toàn bộ banner/slider.
    /// </summary>
    public async Task<List<BannerDto>> GetAllBannersAsync(BannerType? type = null, CancellationToken ct = default)
    {
        var query = _db.Banners
            .AsNoTracking()
            .Include(b => b.Media)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(b => b.Type == type.Value);

        return await query
            .OrderBy(b => b.SortOrder)
            .ThenBy(b => b.CreatedAt)
            .Select(b => new BannerDto(
                b.Id,
                b.MediaId,
                b.Media.Url,
                b.Media.AltText,
                b.Type,
                b.LinkUrl,
                b.SortOrder,
                b.IsActive,
                b.CreatedAt
            ))
            .ToListAsync(ct);
    }

    public async Task<BannerDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var banner = await _db.Banners
            .AsNoTracking()
            .Include(b => b.Media)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (banner == null)
            throw new ValidationException("Banner không tồn tại.");

        return new BannerDto(
            banner.Id,
            banner.MediaId,
            banner.Media.Url,
            banner.Media.AltText,
            banner.Type,
            banner.LinkUrl,
            banner.SortOrder,
            banner.IsActive,
            banner.CreatedAt
        );
    }

    public async Task<BannerDto> CreateAsync(CreateBannerDto dto, CancellationToken ct = default)
    {
        if (dto.SortOrder < 1)
            throw new ValidationException("Thứ tự sắp xếp phải lớn hơn hoặc bằng 1.");

        var mediaId = dto.MediaId;
        if (mediaId == Guid.Empty && !string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            var foundMedia = await _db.MediaLibrary.FirstOrDefaultAsync(m => m.Url == dto.ImageUrl, ct);
            if (foundMedia != null)
            {
                mediaId = foundMedia.Id;
            }
        }

        var media = await _db.MediaLibrary.FirstOrDefaultAsync(m => m.Id == mediaId, ct);
        if (media == null)
            throw new ValidationException("Ảnh liên kết không tồn tại trong thư viện media. Vui lòng tải ảnh lên.");

        var altText = !string.IsNullOrWhiteSpace(dto.AltText) ? dto.AltText.Trim() : (!string.IsNullOrWhiteSpace(dto.Title) ? dto.Title.Trim() : null);
        media.AltText = altText;

        var linkUrl = !string.IsNullOrWhiteSpace(dto.LinkUrl) ? dto.LinkUrl.Trim() : (!string.IsNullOrWhiteSpace(dto.TargetUrl) ? dto.TargetUrl.Trim() : null);

        var banner = new Banner
        {
            Id = Guid.NewGuid(),
            MediaId = mediaId,
            Type = dto.Type,
            LinkUrl = linkUrl,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Banners.Add(banner);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(banner.Id, ct);
    }

    public async Task<BannerDto> UpdateAsync(Guid id, UpdateBannerDto dto, CancellationToken ct = default)
    {
        if (dto.SortOrder < 1)
            throw new ValidationException("Thứ tự sắp xếp phải lớn hơn hoặc bằng 1.");

        var banner = await _db.Banners.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (banner == null)
            throw new ValidationException("Banner không tồn tại.");

        var mediaId = dto.MediaId;
        if (mediaId == Guid.Empty && !string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            var foundMedia = await _db.MediaLibrary.FirstOrDefaultAsync(m => m.Url == dto.ImageUrl, ct);
            if (foundMedia != null)
            {
                mediaId = foundMedia.Id;
            }
        }
        else if (mediaId == Guid.Empty)
        {
            mediaId = banner.MediaId;
        }

        var media = await _db.MediaLibrary.FirstOrDefaultAsync(m => m.Id == mediaId, ct);
        if (media == null)
            throw new ValidationException("Ảnh liên kết không tồn tại trong thư viện media. Vui lòng tải ảnh lên.");

        var altText = !string.IsNullOrWhiteSpace(dto.AltText) ? dto.AltText.Trim() : (!string.IsNullOrWhiteSpace(dto.Title) ? dto.Title.Trim() : null);
        media.AltText = altText;

        var linkUrl = !string.IsNullOrWhiteSpace(dto.LinkUrl) ? dto.LinkUrl.Trim() : (!string.IsNullOrWhiteSpace(dto.TargetUrl) ? dto.TargetUrl.Trim() : null);

        banner.MediaId = mediaId;
        banner.Type = dto.Type;
        banner.LinkUrl = linkUrl;
        banner.SortOrder = dto.SortOrder;
        banner.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(banner.Id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var banner = await _db.Banners.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (banner == null)
            throw new ValidationException("Banner không tồn tại.");

        _db.Banners.Remove(banner);
        await _db.SaveChangesAsync(ct);
    }
}
