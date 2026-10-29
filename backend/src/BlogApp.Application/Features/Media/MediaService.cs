using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Common.Models;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Application.Features.Media;

public class MediaService
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public MediaService(IApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<PagedResult<MediaDto>> GetPagedAsync(MediaFilterDto filter, CancellationToken ct = default)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = filter.PageSize is < 1 or > 100 ? 20 : filter.PageSize;

        var query = _db.MediaLibrary.AsNoTracking();

        if (filter.UsageType.HasValue)
        {
            query = query.Where(x => x.UsageType == filter.UsageType.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(x => x.FileName.ToLower().Contains(search) ||
                                     (x.AltText != null && x.AltText.ToLower().Contains(search)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<MediaDto>(items.Select(ToDto).ToList(), total, page, pageSize);
    }

    public async Task<MediaDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var media = await _db.MediaLibrary.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return media is null ? null : ToDto(media);
    }

    public async Task<MediaDto> UploadAsync(UploadMediaRequest request, CancellationToken ct = default)
    {
        if (request.FileStream == null || request.FileSizeBytes <= 0)
        {
            throw new ValidationException("File tải lên không hợp lệ.");
        }

        if (request.FileSizeBytes > MaxFileSizeBytes)
        {
            throw new ValidationException("Dung lượng file vượt quá giới hạn cho phép (tối đa 10MB).");
        }

        var ext = Path.GetExtension(request.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new ValidationException($"Định dạng file \"{ext}\" không được hỗ trợ. Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .webp, .gif, .svg.");
        }

        var (width, height) = ImageHeaderReader.TryGetDimensions(request.FileStream, ext);

        var url = await _fileStorage.SaveFileAsync(request.FileStream, request.FileName, request.ContentType, ct);

        var media = new MediaLibrary
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName,
            Url = url,
            Width = width,
            Height = height,
            FileSizeBytes = request.FileSizeBytes,
            AltText = request.AltText?.Trim(),
            UsageType = request.UsageType,
            UploadedAt = DateTime.UtcNow
        };

        _db.MediaLibrary.Add(media);
        await _db.SaveChangesAsync(ct);

        return ToDto(media);
    }

    public async Task UpdateAsync(Guid id, UpdateMediaDto dto, CancellationToken ct = default)
    {
        var media = await _db.MediaLibrary.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Ảnh không tồn tại.");

        media.AltText = dto.AltText?.Trim();
        media.UsageType = dto.UsageType;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var media = await _db.MediaLibrary.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ValidationException("Ảnh không tồn tại.");

        if (await _db.Posts.AnyAsync(x => x.ThumbnailMediaId == id || x.OgImageMediaId == id, ct))
        {
            throw new ValidationException("Không thể xoá ảnh đang được bài viết sử dụng làm thumbnail hoặc ảnh OG.");
        }

        if (await _db.Banners.AnyAsync(x => x.MediaId == id, ct))
        {
            throw new ValidationException("Không thể xoá ảnh đang được banner sử dụng.");
        }

        if (await _db.Projects.AnyAsync(x => x.MediaId == id, ct))
        {
            throw new ValidationException("Không thể xoá ảnh đang được dự án sử dụng.");
        }

        await _fileStorage.DeleteFileAsync(media.Url, ct);

        _db.MediaLibrary.Remove(media);
        await _db.SaveChangesAsync(ct);
    }

    private static MediaDto ToDto(MediaLibrary x) =>
        new(x.Id, x.FileName, x.Url, x.Width, x.Height, x.FileSizeBytes, x.AltText, x.UsageType, x.UploadedAt);
}
