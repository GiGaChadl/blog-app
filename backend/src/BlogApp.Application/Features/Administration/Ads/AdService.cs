using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.Administration.Ads;

public class AdService
{
    private readonly IApplicationDbContext _db;

    public AdService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy cấu hình quảng cáo đang hoạt động theo vị trí (công khai).
    /// Kiểm tra IsActive và khoảng thời gian StartDate/EndDate hợp lệ.
    /// </summary>
    public async Task<AdDto?> GetActiveAdByPositionAsync(AdPosition position, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var ad = await _db.Ads
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Position == position &&
                                      a.IsActive &&
                                      (!a.StartDate.HasValue || today >= a.StartDate.Value) &&
                                      (!a.EndDate.HasValue || today <= a.EndDate.Value), ct);

        if (ad == null) return null;

        return new AdDto(ad.Id, ad.Position, ad.AdsenseSlotId, ad.StartDate, ad.EndDate, ad.IsActive);
    }

    /// <summary>
    /// Quản trị viên xem toàn bộ cấu hình quảng cáo theo các vị trí.
    /// </summary>
    public async Task<List<AdDto>> GetAllAdsAsync(CancellationToken ct = default)
    {
        return await _db.Ads
            .AsNoTracking()
            .OrderBy(a => a.Position)
            .Select(a => new AdDto(a.Id, a.Position, a.AdsenseSlotId, a.StartDate, a.EndDate, a.IsActive))
            .ToListAsync(ct);
    }

    public async Task<AdDto?> GetByPositionAsync(AdPosition position, CancellationToken ct = default)
    {
        var ad = await _db.Ads
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Position == position, ct);

        if (ad == null) return null;

        return new AdDto(ad.Id, ad.Position, ad.AdsenseSlotId, ad.StartDate, ad.EndDate, ad.IsActive);
    }

    /// <summary>
    /// Cập nhật hoặc tạo mới cấu hình Google AdSense cho một vị trí (Upsert).
    /// </summary>
    public async Task<AdDto> UpsertAdAsync(AdPosition position, UpsertAdDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.AdsenseSlotId))
            throw new ValidationException("Google AdSense Slot ID không được để trống.");

        if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate > dto.EndDate)
            throw new ValidationException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");

        var ad = await _db.Ads.FirstOrDefaultAsync(a => a.Position == position, ct);

        if (ad == null)
        {
            ad = new Ad
            {
                Id = Guid.NewGuid(),
                Position = position,
                AdsenseSlotId = dto.AdsenseSlotId.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive
            };
            _db.Ads.Add(ad);
        }
        else
        {
            ad.AdsenseSlotId = dto.AdsenseSlotId.Trim();
            ad.StartDate = dto.StartDate;
            ad.EndDate = dto.EndDate;
            ad.IsActive = dto.IsActive;
        }

        await _db.SaveChangesAsync(ct);

        return new AdDto(ad.Id, ad.Position, ad.AdsenseSlotId, ad.StartDate, ad.EndDate, ad.IsActive);
    }

    public async Task DeleteAdAsync(AdPosition position, CancellationToken ct = default)
    {
        var ad = await _db.Ads.FirstOrDefaultAsync(a => a.Position == position, ct);
        if (ad == null)
            throw new ValidationException("Cấu hình quảng cáo tại vị trí này không tồn tại.");

        _db.Ads.Remove(ad);
        await _db.SaveChangesAsync(ct);
    }
}
