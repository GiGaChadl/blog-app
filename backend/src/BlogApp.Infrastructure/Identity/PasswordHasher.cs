using BlogApp.Application.Common.Interfaces;

namespace BlogApp.Infrastructure.Identity;

/// <summary>
/// Dùng BCrypt (hash 1 chiều, có salt tự sinh) cho admin_users.password_hash — KHÁC hẳn
/// smtp_settings.password_encrypted (mã hoá 2 chiều). Xem ghi chú phân biệt hash/encrypt
/// đã bàn kỹ ở phần thiết kế database.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) => BCrypt.Net.BCrypt.HashPassword(plainPassword);

    public bool Verify(string plainPassword, string hash) => BCrypt.Net.BCrypt.Verify(plainPassword, hash);
}
