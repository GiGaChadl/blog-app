using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Common.Models;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.Interactions.Subscribers;

public class SubscriberService
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailSender _emailSender;
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public SubscriberService(IApplicationDbContext db, IEmailSender emailSender)
    {
        _db = db;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Đăng ký nhận tin với cơ chế Double Opt-in.
    /// Ban đầu tạo ở trạng thái Pending và gửi email xác nhận có hạn 7 ngày.
    /// </summary>
    public async Task SubscribeAsync(SubscribeRequest request, string? confirmationBaseUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !EmailRegex.IsMatch(request.Email.Trim()))
            throw new ValidationException("Địa chỉ email không hợp lệ.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existing = await _db.Subscribers
            .FirstOrDefaultAsync(s => s.Email == normalizedEmail, ct);

        if (existing != null && existing.Status == SubscriberStatus.Active)
            throw new ValidationException("Email này đã đăng ký nhận bài viết mới từ trước.");

        var token = Guid.NewGuid().ToString("N");
        var tokenExpiresAt = DateTime.UtcNow.AddDays(7);

        if (existing == null)
        {
            var subscriber = new Subscriber
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                Status = SubscriberStatus.Pending,
                ConfirmToken = token,
                ConfirmTokenExpiresAt = tokenExpiresAt,
                CreatedAt = DateTime.UtcNow
            };
            _db.Subscribers.Add(subscriber);
        }
        else
        {
            // Trường hợp trước đó đang Pending hoặc Unsubscribed: làm mới token để xác nhận lại
            existing.Status = SubscriberStatus.Pending;
            existing.ConfirmToken = token;
            existing.ConfirmTokenExpiresAt = tokenExpiresAt;
        }

        await _db.SaveChangesAsync(ct);

        // Gửi email xác nhận
        var baseUrl = string.IsNullOrWhiteSpace(confirmationBaseUrl) ? "http://localhost:4200" : confirmationBaseUrl.TrimEnd('/');
        var confirmUrl = $"{baseUrl}/api/subscribers/confirm?token={token}";

        var subject = "Xác nhận đăng ký nhận bài viết mới";
        var bodyHtml = $@"
            <div style=""font-family: Arial, sans-serif; line-height: 1.6; max-width: 600px; margin: 0 auto; padding: 24px; color: #1e293b; background: #ffffff; border: 1px solid #e2e8f0; border-radius: 8px;"">
                <h2 style=""color: #2563eb; margin-top: 0; margin-bottom: 16px;"">Xác nhận đăng ký nhận bài viết mới</h2>
                <p>Cảm ơn bạn đã đăng ký nhận bài viết mới từ Blog!</p>
                <p>Vui lòng nhấn vào đường dẫn dưới đây để hoàn tất việc xác nhận đăng ký của bạn:</p>
                
                <p style=""margin: 20px 0; word-break: break-all; font-size: 15px; background: #f8fafc; padding: 14px; border-radius: 6px; border: 1px solid #cbd5e1;"">
                    👉 <a href=""{confirmUrl}"" target=""_blank"" rel=""noopener noreferrer"" style=""color: #2563eb; font-weight: 600; text-decoration: underline;"">{confirmUrl}</a>
                </p>

                <p style=""margin-top: 24px; font-size: 13px; color: #64748b;"">Liên kết này có hiệu lực trong vòng <strong>7 ngày</strong>. Nếu bạn không thực hiện yêu cầu này, bạn có thể bỏ qua email này một cách an toàn.</p>
            </div>";

        await _emailSender.SendEmailAsync(normalizedEmail, subject, bodyHtml, ct);
    }

    /// <summary>
    /// Xác nhận token từ liên kết email, chuyển trạng thái sang Active.
    /// </summary>
    public async Task ConfirmSubscriptionAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ValidationException("Mã xác nhận không được để trống.");

        var subscriber = await _db.Subscribers
            .FirstOrDefaultAsync(s => s.ConfirmToken == token, ct);

        if (subscriber == null)
            throw new ValidationException("Mã xác nhận không hợp lệ hoặc không tồn tại.");

        if (subscriber.ConfirmTokenExpiresAt.HasValue && subscriber.ConfirmTokenExpiresAt.Value < DateTime.UtcNow)
            throw new ValidationException("Liên kết xác nhận đã hết hạn (quá 7 ngày). Vui lòng gửi lại yêu cầu đăng ký.");

        subscriber.Status = SubscriberStatus.Active;
        subscriber.ConfirmedAt = DateTime.UtcNow;
        subscriber.ConfirmToken = null;
        subscriber.ConfirmTokenExpiresAt = null;

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Hủy đăng ký nhận tin. Chuyển trạng thái sang Unsubscribed.
    /// </summary>
    public async Task UnsubscribeAsync(UnsubscribeRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Email không được để trống.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var subscriber = await _db.Subscribers
            .FirstOrDefaultAsync(s => s.Email == normalizedEmail, ct);

        if (subscriber == null || subscriber.Status == SubscriberStatus.Unsubscribed)
            return; // Idempotent

        subscriber.Status = SubscriberStatus.Unsubscribed;
        subscriber.UnsubscribedAt = DateTime.UtcNow;
        subscriber.ConfirmToken = null;
        subscriber.ConfirmTokenExpiresAt = null;

        await _db.SaveChangesAsync(ct);
    }

    // ==========================================
    // Quản trị viên (Admin) Endpoints
    // ==========================================

    /// <summary>
    /// Quản trị viên xem danh sách người đăng ký phân trang và lọc theo trạng thái.
    /// </summary>
    public async Task<PagedResult<SubscriberDto>> GetSubscribersAsync(SubscriberStatus? status, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Subscribers.AsNoTracking().AsQueryable();

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SubscriberDto(
                s.Id,
                s.Email,
                s.Status,
                s.CreatedAt,
                s.ConfirmedAt,
                s.UnsubscribedAt
            ))
            .ToListAsync(ct);

        return new PagedResult<SubscriberDto>(items, total, page, pageSize);
    }

    /// <summary>
    /// Quản trị viên xóa người đăng ký khỏi danh sách.
    /// </summary>
    public async Task DeleteSubscriberAsync(Guid id, CancellationToken ct = default)
    {
        var subscriber = await _db.Subscribers.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (subscriber == null)
            throw new ValidationException("Người đăng ký không tồn tại.");

        _db.Subscribers.Remove(subscriber);
        await _db.SaveChangesAsync(ct);
    }
}
