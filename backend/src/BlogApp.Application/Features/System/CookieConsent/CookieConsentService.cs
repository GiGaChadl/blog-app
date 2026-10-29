using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.System.CookieConsent;

public class CookieConsentService
{
    private readonly IApplicationDbContext _db;

    public CookieConsentService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Ghi nhận lựa chọn cookie của độc giả theo nguyên tắc tối thiểu hoá dữ liệu (PROJECT-MASTER-GUIDE §10):
    /// - Từ chối lần đầu: KHÔNG tạo dòng nào trong DB.
    /// - Đồng ý lần đầu: Tạo bản ghi với Choice = Accepted.
    /// - Chủ động đổi ý/thu hồi sau này: Cập nhật bản ghi tương ứng.
    /// </summary>
    public async Task<ConsentStatusDto> RecordConsentAsync(RecordConsentRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.VisitorToken))
            throw new ValidationException("Visitor token không được để trống.");

        var token = request.VisitorToken.Trim();
        var existing = await _db.CookieConsents
            .FirstOrDefaultAsync(c => c.VisitorToken == token, ct);

        if (existing == null)
        {
            // Nếu là từ chối lần đầu và chưa có trong DB: KHÔNG tạo dòng nào
            if (request.Choice == CookieChoice.Declined)
            {
                return new ConsentStatusDto(token, HasConsented: false, Choice: null, ConsentedAt: null);
            }

            // Đồng ý lần đầu: Tạo bản ghi
            var consent = new Domain.Entities.CookieConsent
            {
                Id = Guid.NewGuid(),
                VisitorToken = token,
                Choice = request.Choice,
                ConsentedAt = DateTime.UtcNow
            };
            _db.CookieConsents.Add(consent);
            await _db.SaveChangesAsync(ct);

            return new ConsentStatusDto(token, HasConsented: true, Choice: consent.Choice, ConsentedAt: consent.ConsentedAt);
        }

        // Đã có bản ghi từ trước: Cập nhật lựa chọn mới (cho phép thu hồi hoặc đồng ý lại)
        existing.Choice = request.Choice;
        existing.ConsentedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new ConsentStatusDto(token, HasConsented: true, Choice: existing.Choice, ConsentedAt: existing.ConsentedAt);
    }

    /// <summary>
    /// Kiểm tra trạng thái đồng ý cookie của một visitor token.
    /// </summary>
    public async Task<ConsentStatusDto> GetStatusAsync(string visitorToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(visitorToken))
            return new ConsentStatusDto(string.Empty, HasConsented: false, Choice: null, ConsentedAt: null);

        var token = visitorToken.Trim();
        var consent = await _db.CookieConsents
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.VisitorToken == token, ct);

        if (consent == null)
            return new ConsentStatusDto(token, HasConsented: false, Choice: null, ConsentedAt: null);

        return new ConsentStatusDto(token, HasConsented: true, Choice: consent.Choice, ConsentedAt: consent.ConsentedAt);
    }

    /// <summary>
    /// Tiện ích kiểm tra xem độc giả đã bấm Đồng ý hay chưa (để quyết định ghi log truy cập).
    /// </summary>
    public async Task<bool> HasAcceptedAsync(string visitorToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(visitorToken))
            return false;

        var token = visitorToken.Trim();
        return await _db.CookieConsents
            .AnyAsync(c => c.VisitorToken == token && c.Choice == CookieChoice.Accepted, ct);
    }
}
