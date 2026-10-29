using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Application.Features.Administration.Contact;

public class ContactInfoService
{
    private readonly IApplicationDbContext _db;
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public ContactInfoService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy thông tin liên hệ và danh sách mạng xã hội của Blog (công khai).
    /// </summary>
    public async Task<ContactInfoDto> GetContactInfoAsync(CancellationToken ct = default)
    {
        var info = await _db.ContactInfo
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (info == null)
        {
            return new ContactInfoDto(
                Email: "admin@blog.local",
                Location: null,
                Address: null,
                Phone: null,
                Bio: null,
                About: null,
                Copyright: null,
                FacebookUrl: null,
                GithubUrl: null,
                LinkedinUrl: null,
                YoutubeUrl: null,
                AuthorName: null,
                Slogan: null,
                SocialLinks: new List<SocialLinkDto>(),
                UpdatedAt: DateTime.UtcNow
            );
        }

        var (phone, bio, about, copyright, authorName, slogan, socialLinks) = ParseMetadata(info.SocialLinksJson);
        var facebookUrl = socialLinks.FirstOrDefault(x => string.Equals(x?.Platform, "facebook", StringComparison.OrdinalIgnoreCase))?.Url;
        var githubUrl = socialLinks.FirstOrDefault(x => string.Equals(x?.Platform, "github", StringComparison.OrdinalIgnoreCase))?.Url;
        var linkedinUrl = socialLinks.FirstOrDefault(x => string.Equals(x?.Platform, "linkedin", StringComparison.OrdinalIgnoreCase))?.Url;
        var youtubeUrl = socialLinks.FirstOrDefault(x => string.Equals(x?.Platform, "youtube", StringComparison.OrdinalIgnoreCase))?.Url;

        return new ContactInfoDto(
            Email: info.Email,
            Location: info.Location,
            SocialLinks: socialLinks,
            UpdatedAt: info.UpdatedAt,
            Address: info.Location,
            Phone: phone,
            Bio: bio,
            About: about,
            Copyright: copyright,
            FacebookUrl: facebookUrl,
            GithubUrl: githubUrl,
            LinkedinUrl: linkedinUrl,
            YoutubeUrl: youtubeUrl,
            AuthorName: authorName,
            Slogan: slogan
        );
    }

    /// <summary>
    /// Cập nhật thông tin liên hệ và danh sách mạng xã hội (Upsert bản ghi singleton).
    /// Hợp nhất dữ liệu an toàn để không làm mất cấu hình SEO được lưu chung trong JSONB.
    /// </summary>
    public async Task<ContactInfoDto> UpdateContactInfoAsync(UpdateContactInfoDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || !EmailRegex.IsMatch(dto.Email.Trim()))
            throw new ValidationException("Địa chỉ email liên hệ không hợp lệ.");

        var location = !string.IsNullOrWhiteSpace(dto.Address)
            ? dto.Address.Trim()
            : (!string.IsNullOrWhiteSpace(dto.Location) ? dto.Location.Trim() : null);

        var info = await _db.ContactInfo.FirstOrDefaultAsync(ct);

        // Khởi tạo hoặc đọc JsonObject hiện có để bảo toàn các thuộc tính khác (siteName, siteLogoUrl...)
        JsonObject node;
        try
        {
            node = !string.IsNullOrWhiteSpace(info?.SocialLinksJson)
                ? JsonNode.Parse(info.SocialLinksJson)?.AsObject() ?? new JsonObject()
                : new JsonObject();
        }
        catch
        {
            node = new JsonObject();
        }

        var links = dto.SocialLinks != null ? new List<SocialLinkDto>(dto.SocialLinks) : new List<SocialLinkDto>();

        void UpsertLink(string platform, string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                links.RemoveAll(x => string.Equals(x?.Platform, platform, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                var idx = links.FindIndex(x => string.Equals(x?.Platform, platform, StringComparison.OrdinalIgnoreCase));
                var item = new SocialLinkDto(platform, url.Trim());
                if (idx >= 0) links[idx] = item;
                else links.Add(item);
            }
        }

        if (dto.FacebookUrl != null) UpsertLink("Facebook", dto.FacebookUrl);
        if (dto.GithubUrl != null) UpsertLink("GitHub", dto.GithubUrl);
        if (dto.LinkedinUrl != null) UpsertLink("LinkedIn", dto.LinkedinUrl);
        if (dto.YoutubeUrl != null) UpsertLink("YouTube", dto.YoutubeUrl);

        if (dto.Phone != null) node["phone"] = dto.Phone.Trim();
        if (dto.Bio != null) node["bio"] = dto.Bio.Trim();
        if (dto.About != null) node["about"] = dto.About.Trim();
        if (dto.Copyright != null) node["copyright"] = dto.Copyright.Trim();
        if (dto.AuthorName != null) node["authorName"] = dto.AuthorName.Trim();
        if (dto.Slogan != null) node["slogan"] = dto.Slogan.Trim();

        node["links"] = JsonSerializer.SerializeToNode(links, JsonOptions);

        var jsonMeta = node.ToJsonString();

        if (info == null)
        {
            info = new ContactInfo
            {
                Id = Guid.NewGuid(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                Location = location,
                SocialLinksJson = jsonMeta,
                UpdatedAt = DateTime.UtcNow
            };
            _db.ContactInfo.Add(info);
        }
        else
        {
            info.Email = dto.Email.Trim().ToLowerInvariant();
            info.Location = location;
            info.SocialLinksJson = jsonMeta;
            info.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        return await GetContactInfoAsync(ct);
    }

    private static (string? Phone, string? Bio, string? About, string? Copyright, string? AuthorName, string? Slogan, List<SocialLinkDto> Links) ParseMetadata(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return (null, null, null, null, null, null, new List<SocialLinkDto>());

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                string? phone = doc.RootElement.TryGetProperty("phone", out var p) ? p.GetString() : null;
                string? bio = doc.RootElement.TryGetProperty("bio", out var b) ? b.GetString() : null;
                string? about = doc.RootElement.TryGetProperty("about", out var a) ? a.GetString() : null;
                string? copyright = doc.RootElement.TryGetProperty("copyright", out var c) ? c.GetString() : null;
                string? authorName = doc.RootElement.TryGetProperty("authorName", out var an)
                    ? an.GetString()
                    : (doc.RootElement.TryGetProperty("author", out var a1) ? a1.GetString() : null);
                string? slogan = doc.RootElement.TryGetProperty("slogan", out var sl) ? sl.GetString() : null;

                List<SocialLinkDto> links = new();
                if (doc.RootElement.TryGetProperty("links", out var l) && l.ValueKind == JsonValueKind.Array)
                {
                    links = JsonSerializer.Deserialize<List<SocialLinkDto>>(l.GetRawText(), JsonOptions) ?? new List<SocialLinkDto>();
                }

                return (phone, bio, about, copyright, authorName, slogan, links);
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var links = JsonSerializer.Deserialize<List<SocialLinkDto>>(json, JsonOptions) ?? new List<SocialLinkDto>();
                return (null, null, null, null, null, null, links);
            }
        }
        catch
        {
            // ignored
        }

        return (null, null, null, null, null, null, new List<SocialLinkDto>());
    }
}
