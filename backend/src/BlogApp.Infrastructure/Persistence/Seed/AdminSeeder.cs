using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlogApp.Infrastructure.Persistence.Seed;

/// <summary>
/// Tạo tài khoản admin ĐẦU TIÊN nếu bảng admin_users đang trống — chỉ chạy 1 lần duy nhất
/// (tự kiểm tra .AnyAsync() trước). Thông tin lấy từ appsettings section "SeedAdmin", KHÔNG
/// hard-code trong code. Sau khi seed xong, đổi mật khẩu ngay qua tính năng thật (chưa làm ở
/// đợt này) — mật khẩu trong appsettings chỉ dùng đúng 1 lần cho lần chạy đầu.
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger logger)
    {
        if (await db.AdminUsers.AnyAsync())
        {
            return; // đã có admin — không seed lại, tránh tạo trùng mỗi lần khởi động app
        }

        var username = configuration["SeedAdmin:Username"];
        var password = configuration["SeedAdmin:Password"];
        var email = configuration["SeedAdmin:Email"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning(
                "Bỏ qua seed admin — thiếu cấu hình SeedAdmin:Username/Password/Email trong appsettings.json. " +
                "Bảng admin_users đang trống, KHÔNG đăng nhập được cho tới khi có ít nhất 1 dòng.");
            return;
        }

        var admin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHasher.Hash(password),
            CreatedAt = DateTime.UtcNow
        };

        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();

        logger.LogWarning(
            "Đã seed tài khoản admin đầu tiên: {Username}. ĐỔI MẬT KHẨU NÀY NGAY sau khi có " +
            "tính năng đổi mật khẩu — mật khẩu hiện tại đang nằm dạng plain-text trong appsettings.json.",
            username);
    }
}
