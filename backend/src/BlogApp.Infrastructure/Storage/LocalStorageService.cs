using BlogApp.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BlogApp.Infrastructure.Storage;

public class LocalStorageService : IFileStorageService
{
    private readonly string _uploadsFolder;

    public LocalStorageService(IConfiguration configuration)
    {
        var root = configuration["Storage:WebRootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        _uploadsFolder = Path.Combine(root, "uploads");
        if (!Directory.Exists(_uploadsFolder))
        {
            Directory.CreateDirectory(_uploadsFolder);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(_uploadsFolder, uniqueFileName);

        using (var destStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(destStream, ct);
        }

        return $"/uploads/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return Task.CompletedTask;

        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(_uploadsFolder, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
