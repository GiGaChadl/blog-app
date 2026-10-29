using BlogApp.Infrastructure.Services;
using Xunit;

namespace BlogApp.Application.Tests;

public class AesEncryptionServiceTests
{
    private AesEncryptionService CreateService(string? encryptionKey = "My-Secret-Custom-AES-Key-For-Testing-12345!")
    {
        return new AesEncryptionService(encryptionKey);
    }

    [Fact]
    public void EncryptAndDecrypt_RoundTrip_ReturnsOriginalText()
    {
        var service = CreateService();
        var original = "SmtpSecretPassword_2026!@#";

        var cipher = service.Encrypt(original);
        Assert.NotEqual(original, cipher);
        Assert.NotEmpty(cipher);

        var decrypted = service.Decrypt(cipher);
        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void Encrypt_SameTextTwice_ProducesDifferentCipherTextsDueToRandomIv()
    {
        var service = CreateService();
        var text = "SecurePassword123";

        var cipher1 = service.Encrypt(text);
        var cipher2 = service.Encrypt(text);

        // Mỗi lần mã hoá sinh IV ngẫu nhiên nên chuỗi cipher phải khác nhau
        Assert.NotEqual(cipher1, cipher2);

        // Nhưng khi giải mã cả 2 đều trả về chuỗi gốc
        Assert.Equal(text, service.Decrypt(cipher1));
        Assert.Equal(text, service.Decrypt(cipher2));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EncryptAndDecrypt_EmptyOrNull_ReturnsEmptyString(string? text)
    {
        var service = CreateService();

        var cipher = service.Encrypt(text!);
        Assert.Equal(string.Empty, cipher);

        var decrypted = service.Decrypt(cipher);
        Assert.Equal(string.Empty, decrypted);
    }
}
