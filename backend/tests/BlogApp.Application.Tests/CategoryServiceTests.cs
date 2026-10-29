using BlogApp.Application.Features.Categories;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Application.Tests;

public class CategoryServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenParentIsLevelThree_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var parent = new Category { Id = Guid.NewGuid(), Name = "Cháu", Slug = "chau", Level = 3 };
        db.Categories.Add(parent);
        await db.SaveChangesAsync();
        var service = new CategoryService(db);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateCategoryDto("Cấp bốn", "cap-bon", parent.Id)));

        Assert.Equal("Không thể tạo danh mục mới - danh mục cha đã ở cấp 3 (tối đa 3 cấp).", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenSlugAlreadyExists_ThrowsValidationException()
    {
        await using var db = CreateContext();
        db.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Existing", Slug = "existing" });
        await db.SaveChangesAsync();
        var service = new CategoryService(db);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateCategoryDto("New", "existing", null)));
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryHasSubcategories_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var categoryId = Guid.NewGuid();
        db.Categories.AddRange(
            new Category { Id = categoryId, Name = "Parent", Slug = "parent" },
            new Category { Id = Guid.NewGuid(), Name = "Child", Slug = "child", ParentId = categoryId });
        await db.SaveChangesAsync();
        var service = new CategoryService(db);

        await Assert.ThrowsAsync<ValidationException>(() => service.DeleteAsync(categoryId));

        Assert.NotNull(await db.Categories.FindAsync(categoryId));
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryHasPosts_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Category { Id = categoryId, Name = "Leaf", Slug = "leaf" });
        db.Posts.Add(new Post
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Title = "Test Post",
            Slug = "test-post",
            ThumbnailMediaId = Guid.NewGuid()
        });
        await db.SaveChangesAsync();
        var service = new CategoryService(db);

        await Assert.ThrowsAsync<ValidationException>(() => service.DeleteAsync(categoryId));

        Assert.NotNull(await db.Categories.FindAsync(categoryId));
    }

    [Fact]
    public async Task CreateAsync_WhenSlugIsReservedTree_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var service = new CategoryService(db);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateCategoryDto("Tree", "tree", null)));

        Assert.Contains("bảo lưu", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryNotFound_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var service = new CategoryService(db);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategorySuccessfully()
    {
        await using var db = CreateContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "To Delete", Slug = "to-delete" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        var service = new CategoryService(db);

        await service.DeleteAsync(category.Id);

        Assert.Null(await db.Categories.FindAsync(category.Id));
    }

    [Fact]
    public async Task CreateAsync_WhenSortOrderLessThanOne_ThrowsValidationException()
    {
        await using var db = CreateContext();
        var service = new CategoryService(db);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateCategoryDto("Cat", "cat", null, 0)));

        Assert.Equal("Thứ tự sắp xếp phải lớn hơn hoặc bằng 1.", ex.Message);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsHierarchicalOrder()
    {
        await using var db = CreateContext();
        var rootA = new Category { Id = Guid.NewGuid(), Name = "Root A", Slug = "root-a", Level = 1, SortOrder = 2 };
        var rootB = new Category { Id = Guid.NewGuid(), Name = "Root B", Slug = "root-b", Level = 1, SortOrder = 1 };
        var childB1 = new Category { Id = Guid.NewGuid(), Name = "Child B1", Slug = "child-b1", Level = 2, ParentId = rootB.Id, SortOrder = 2 };
        var childB2 = new Category { Id = Guid.NewGuid(), Name = "Child B2", Slug = "child-b2", Level = 2, ParentId = rootB.Id, SortOrder = 1 };
        var grandChildB2 = new Category { Id = Guid.NewGuid(), Name = "GrandChild", Slug = "grandchild", Level = 3, ParentId = childB2.Id, SortOrder = 1 };

        db.Categories.AddRange(rootA, rootB, childB1, childB2, grandChildB2);
        await db.SaveChangesAsync();

        var service = new CategoryService(db);
        var result = await service.GetAllAsync();

        // Expected order:
        // 1. Root B (SortOrder 1)
        // 2. Child B2 (SortOrder 1)
        // 3. GrandChild (child of B2)
        // 4. Child B1 (SortOrder 2)
        // 5. Root A (SortOrder 2)
        Assert.Equal(5, result.Count);
        Assert.Equal("Root B", result[0].Name);
        Assert.Equal("Child B2", result[1].Name);
        Assert.Equal("GrandChild", result[2].Name);
        Assert.Equal("Child B1", result[3].Name);
        Assert.Equal("Root A", result[4].Name);
    }

    private static TestApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestApplicationDbContext(options);
    }
}
