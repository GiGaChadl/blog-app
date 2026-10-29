using BlogApp.Domain.Entities;

namespace BlogApp.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(AdminUser admin);
}
