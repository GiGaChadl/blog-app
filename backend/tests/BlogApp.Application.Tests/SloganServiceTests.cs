using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Features.Administration.Slogans;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class SloganServiceTests
{
    private (TestApplicationDbContext Db, SloganService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var service = new SloganService(db);

        return (db, service);
    }

    [Fact]
    public async Task CreateAsync_WhenActive_DeactivatesPreviousActiveSlogans()
    {
        var (db, service) = CreateService();

        var oldSlogan = new Slogan
        {
            Id = Guid.NewGuid(),
            Content = "Slogan cũ đang chạy",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        db.Slogans.Add(oldSlogan);
        await db.SaveChangesAsync();

        // Tạo slogan mới và active
        var dto = new CreateSloganDto("Slogan mới hiện đại", IsActive: true);
        var created = await service.CreateAsync(dto);

        Assert.True(created.IsActive);
        Assert.Equal("Slogan mới hiện đại", created.Content);

        // Kiểm tra slogan cũ đã bị deactivate
        await db.Entry(oldSlogan).ReloadAsync();
        Assert.False(oldSlogan.IsActive);

        // Số lượng slogan active trong DB phải đúng là 1
        var activeCount = await db.Slogans.CountAsync(s => s.IsActive);
        Assert.Equal(1, activeCount);
    }

    [Fact]
    public async Task UpdateAsync_WhenSetToActive_DeactivatesOtherActiveSlogans()
    {
        var (db, service) = CreateService();

        var currentActive = new Slogan
        {
            Id = Guid.NewGuid(),
            Content = "Đang active",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var inactive = new Slogan
        {
            Id = Guid.NewGuid(),
            Content = "Đang tắt",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        db.Slogans.AddRange(currentActive, inactive);
        await db.SaveChangesAsync();

        // Bật active cho cái đang tắt
        var updateDto = new UpdateSloganDto("Bật lên thành active", IsActive: true);
        await service.UpdateAsync(inactive.Id, updateDto);

        await db.Entry(currentActive).ReloadAsync();
        await db.Entry(inactive).ReloadAsync();

        Assert.False(currentActive.IsActive);
        Assert.True(inactive.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_EmptyContent_ThrowsValidationException(string content)
    {
        var (_, service) = CreateService();

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateSloganDto(content, true)));
    }

    [Fact]
    public async Task GetActiveSloganAsync_ReturnsSingleActive()
    {
        var (db, service) = CreateService();

        db.Slogans.AddRange(
            new Slogan { Id = Guid.NewGuid(), Content = "Active one", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Slogan { Id = Guid.NewGuid(), Content = "Inactive one", IsActive = false, CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var active = await service.GetActiveSloganAsync();
        Assert.NotNull(active);
        Assert.Equal("Active one", active.Content);
    }
}
