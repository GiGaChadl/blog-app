using System.Security.Cryptography;
using System.Text;
using BlogApp.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlogApp.Infrastructure.Services;

/// <summary>
/// Cài đặt mã hoá 2 chiều AES-256 đối xứng cho mật khẩu SMTP (smtp_settings.password_encrypted).
/// Tự động sinh IV (Initialization Vector) ngẫu nhiên cho mỗi lần mã hoá để chống tấn công phân tích mẫu.
/// Khoá mã hoá được nạp từ biến môi trường hoặc cấu hình Security:EncryptionKey (KHÔNG lưu trong DB).
/// </summary>
public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    [ActivatorUtilitiesConstructor]
    public AesEncryptionService(IConfiguration configuration) 
        : this(configuration["Security:EncryptionKey"] ?? "BlogApp-Default-AES-Encryption-Key-2026-Secure-FallBack!")
    {
    }

    public AesEncryptionService(string customKey)
    {
        var rawKey = string.IsNullOrWhiteSpace(customKey)
            ? "BlogApp-Default-AES-Encryption-Key-2026-Secure-FallBack!"
            : customKey;

        // Băm SHA-256 để luôn đảm bảo khoá có kích thước chuẩn 256-bit (32 bytes)
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        var iv = aes.IV;

        using var encryptor = aes.CreateEncryptor(aes.Key, iv);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Ghép [16 bytes IV] + [cipherBytes]
        var combined = new byte[iv.Length + cipherBytes.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, iv.Length, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        var combined = Convert.FromBase64String(cipherText);
        if (combined.Length < 16)
            throw new CryptographicException("Dữ liệu mã hoá không hợp lệ.");

        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[16];
        Buffer.BlockCopy(combined, 0, iv, 0, 16);
        aes.IV = iv;

        var cipherBytes = new byte[combined.Length - 16];
        Buffer.BlockCopy(combined, 16, cipherBytes, 0, cipherBytes.Length);

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
