using BlogApp.Application.Features.Tags;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Application.Tests;

public class TagServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ThrowsValidationException()
    {
        await using var db = CreateContext();
        db.Tags.Add(new Tag { Id = Guid.NewGuid(), Name = "CSharp", Slug = "csharp" });
        await db.SaveChangesAsync();
        var service = new TagService(db);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateTagDto("CSharp", "dotnet")));
    }

    [Fact]
    public async Task CreateAsync_WhenSlugAlreadyExists_ThrowsValidationException()
    {
        await using var db = CreateContext();
        db.Tags.Add(new Tag { Id = Guid.NewGuid(), Name = "CSharp", Slug = "csharp" });
        await db.SaveChangesAsync();
        var service = new TagService(db);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateTagDto("DotNet", "csharp")));
    }

    [Fact]
    public async Task UpdateAsync_WhenNameOrSlugBelongsToAnotherTag_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var first = new Tag { Id = Guid.NewGuid(), Name = "CSharp", Slug = "csharp" };
        var second = new Tag { Id = Guid.NewGuid(), Name = "DotNet", Slug = "dotnet" };
        db.Tags.AddRange(first, second);
        await db.SaveChangesAsync();
        var service = new TagService(db);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateAsync(second.Id, new UpdateTagDto("CSharp", "different")));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateAsync(second.Id, new UpdateTagDto("Different", "csharp")));
    }

    [Fact]
    public async Task DeleteAsync_RemovesTag()
    {
        await using var db = CreateContext();
        var tag = new Tag { Id = Guid.NewGuid(), Name = "CSharp", Slug = "csharp" };
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        var service = new TagService(db);

        await service.DeleteAsync(tag.Id);

        Assert.Null(await db.Tags.FindAsync(tag.Id));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        await using var db = CreateContext();
        db.Tags.AddRange(
            new Tag { Id = Guid.NewGuid(), Name = "A-Tag", Slug = "a-tag" },
            new Tag { Id = Guid.NewGuid(), Name = "B-Tag", Slug = "b-tag" },
            new Tag { Id = Guid.NewGuid(), Name = "C-Tag", Slug = "c-tag" }
        );
        await db.SaveChangesAsync();
        var service = new TagService(db);

        var result = await service.GetAllAsync(page: 1, pageSize: 2);

        Assert.Equal(3, result.Total);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("A-Tag", result.Items[0].Name);
    }

    [Fact]
    public async Task CreateAsync_TrimsAndLowercasesSlugAndName()
    {
        await using var db = CreateContext();
        var service = new TagService(db);

        var result = await service.CreateAsync(new CreateTagDto("   Angular Core   ", "   ANGULAR-CORE   "));

        Assert.Equal("Angular Core", result.Name);
        Assert.Equal("angular-core", result.Slug);
        var inDb = await db.Tags.FindAsync(result.Id);
        Assert.NotNull(inDb);
        Assert.Equal("Angular Core", inDb.Name);
        Assert.Equal("angular-core", inDb.Slug);
    }

    [Theory]
    [InlineData("", "slug")]
    [InlineData("   ", "slug")]
    [InlineData("name", "")]
    [InlineData("name", "   ")]
    public async Task CreateAsync_WhenNameOrSlugEmpty_ThrowsValidationException(string name, string slug)
    {
        await using var db = CreateContext();
        var service = new TagService(db);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateTagDto(name, slug)));
    }

    [Fact]
    public async Task DeleteAsync_WhenTagNotFound_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var service = new TagService(db);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesNameAndSlugSuccessfully()
    {
        await using var db = CreateContext();
        var tag = new Tag { Id = Guid.NewGuid(), Name = "Old Name", Slug = "old-slug" };
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        var service = new TagService(db);

        await service.UpdateAsync(tag.Id, new UpdateTagDto("  New Name  ", "  NEW-SLUG  "));

        var updated = await db.Tags.FindAsync(tag.Id);
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.Name);
        Assert.Equal("new-slug", updated.Slug);
    }

    [Fact]
    public async Task GetBySlugAsync_WhenExists_ReturnsTagDto()
    {
        await using var db = CreateContext();
        var tag = new Tag { Id = Guid.NewGuid(), Name = "React", Slug = "react" };
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        var service = new TagService(db);

        var result = await service.GetBySlugAsync("react");

        Assert.NotNull(result);
        Assert.Equal("React", result.Name);
        Assert.Equal("react", result.Slug);
    }

    [Fact]
    public async Task GetBySlugAsync_WhenDoesNotExist_ReturnsNull()
    {
        await using var db = CreateContext();
        var service = new TagService(db);

        var result = await service.GetBySlugAsync("non-existent");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_NormalizesInvalidPageAndPageSize()
    {
        await using var db = CreateContext();
        db.Tags.Add(new Tag { Id = Guid.NewGuid(), Name = "Tag 1", Slug = "tag-1" });
        await db.SaveChangesAsync();
        var service = new TagService(db);

        var result = await service.GetAllAsync(page: -5, pageSize: 500);

        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Single(result.Items);
    }

    private static TestApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestApplicationDbContext(options);
    }
}
