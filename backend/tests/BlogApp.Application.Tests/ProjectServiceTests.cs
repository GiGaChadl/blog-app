using System.ComponentModel.DataAnnotations;
using BlogApp.Application.Features.Administration.Projects;
using BlogApp.Domain.Entities;
using BlogApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogApp.Application.Tests;

public class ProjectServiceTests
{
    private (TestApplicationDbContext Db, ProjectService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new TestApplicationDbContext(options);
        var service = new ProjectService(db);

        return (db, service);
    }

    [Fact]
    public async Task CreateAsync_InvalidMediaId_ThrowsValidationException()
    {
        var (_, service) = CreateService();

        var dto = new CreateProjectDto(Guid.NewGuid(), "Project 1", "Desc", "C#");
        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ValidMediaId_CreatesProject()
    {
        var (db, service) = CreateService();

        var media = new MediaLibrary
        {
            Id = Guid.NewGuid(),
            FileName = "project.jpg",
            Url = "/uploads/project.jpg",
            UsageType = MediaUsageType.ProjectImage,
            UploadedAt = DateTime.UtcNow
        };
        db.MediaLibrary.Add(media);
        await db.SaveChangesAsync();

        var dto = new CreateProjectDto(media.Id, "Hệ Thống Blog", "Mô tả dự án", ".NET 10", 1);
        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Hệ Thống Blog", result.Title);
        Assert.Equal(media.Id, result.MediaId);
        Assert.Equal(1, result.SortOrder);
    }

    [Fact]
    public async Task GetProjectsAsync_ReturnsOrderedBySortOrder()
    {
        var (db, service) = CreateService();

        var media = new MediaLibrary { Id = Guid.NewGuid(), FileName = "p.png", Url = "/p.png", UploadedAt = DateTime.UtcNow };
        db.MediaLibrary.Add(media);

        db.Projects.AddRange(
            new Project { Id = Guid.NewGuid(), MediaId = media.Id, Title = "P2", SortOrder = 2 },
            new Project { Id = Guid.NewGuid(), MediaId = media.Id, Title = "P1", SortOrder = 1 }
        );
        await db.SaveChangesAsync();

        var list = await service.GetProjectsAsync();
        Assert.Equal(2, list.Count);
        Assert.Equal("P1", list[0].Title);
        Assert.Equal("P2", list[1].Title);
    }

    [Fact]
    public async Task CreateAsync_WhenSortOrderLessThanOne_ThrowsValidationException()
    {
        var (db, service) = CreateService();
        var media = new MediaLibrary { Id = Guid.NewGuid(), FileName = "p.png", Url = "/p.png", UploadedAt = DateTime.UtcNow };
        db.MediaLibrary.Add(media);
        await db.SaveChangesAsync();

        var dto = new CreateProjectDto(media.Id, "P", "Desc", "Tag", 0);
        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(dto));
        Assert.Contains("Thứ tự sắp xếp phải lớn hơn hoặc bằng 1", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_ExistingProject_DeletesSuccessfully()
    {
        var (db, service) = CreateService();

        var media = new MediaLibrary { Id = Guid.NewGuid(), FileName = "p.png", Url = "/p.png", UploadedAt = DateTime.UtcNow };
        db.MediaLibrary.Add(media);

        var project = new Project { Id = Guid.NewGuid(), MediaId = media.Id, Title = "Xóa tôi đi" };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        await service.DeleteAsync(project.Id);

        var exists = await db.Projects.AnyAsync(p => p.Id == project.Id);
        Assert.False(exists);
    }
}
