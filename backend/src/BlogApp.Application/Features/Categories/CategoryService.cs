using BlogApp.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using BlogApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlogApp.Application.Features.Categories
{
    public class CategoryService
    {
        private readonly IApplicationDbContext _db;
        public CategoryService(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<CategoryTreeDto>> GetTreeAsync(CancellationToken ct = default)
        {
            var all = await _db.Categories.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync(ct);

            List<CategoryTreeDto> BuildChildren(Guid? parentId) =>
                all.Where(x => x.ParentId == parentId)
                   .OrderBy(x => x.SortOrder)
                   .ThenBy(x => x.Name)
                   .Select(x => new CategoryTreeDto(x.Id, x.Name, x.Slug, x.Level, BuildChildren(x.Id)))
                   .ToList();

            return BuildChildren(null);
        }

        public async Task<List<CategoryDto>> GetAllAsync(CancellationToken ct = default)
        {
            var all = await _db.Categories.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync(ct);
            var parentMap = all.ToDictionary(x => x.Id, x => x.Name);

            var result = new List<CategoryDto>();
            void Traverse(Guid? parentId)
            {
                var children = all.Where(x => x.ParentId == parentId).OrderBy(x => x.SortOrder).ThenBy(x => x.Name);
                foreach (var child in children)
                {
                    var parentName = child.ParentId.HasValue && parentMap.TryGetValue(child.ParentId.Value, out var pName) ? pName : null;
                    result.Add(ToDto(child, parentName));
                    Traverse(child.Id);
                }
            }
            Traverse(null);

            if (result.Count < all.Count)
            {
                var addedIds = result.Select(x => x.Id).ToHashSet();
                foreach (var c in all.Where(x => !addedIds.Contains(x.Id)).OrderBy(x => x.SortOrder).ThenBy(x => x.Name))
                {
                    var parentName = c.ParentId.HasValue && parentMap.TryGetValue(c.ParentId.Value, out var pName) ? pName : null;
                    result.Add(ToDto(c, parentName));
                }
            }

            return result;
        }

        public async Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
        {
            var normalizedSlug = slug?.Trim().ToLowerInvariant() ?? string.Empty;
            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Slug.ToLower() == normalizedSlug, ct);
            if (c is null) return null;
            string? parentName = null;
            if (c.ParentId.HasValue)
            {
                parentName = await _db.Categories.Where(x => x.Id == c.ParentId.Value).Select(x => x.Name).FirstOrDefaultAsync(ct);
            }
            return ToDto(c, parentName);
        }

        public static CategoryDto ToDto(Category c, string? parentName = null) =>
            new(c.Id, c.Name, c.Slug, c.ParentId, c.Level, c.SortOrder, parentName);

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
        {
            if (dto.SortOrder < 1)
                throw new ValidationException("Thứ tự sắp xếp phải lớn hơn hoặc bằng 1.");

            var normalizedSlug = dto.Slug?.Trim().ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedSlug))
                throw new ValidationException("Slug không được để trống.");

            if (dto.ParentId.HasValue)
            {
                var parent = await _db.Categories.FirstOrDefaultAsync(x => x.Id == dto.ParentId.Value, ct)
                    ?? throw new ValidationException("Danh mục cha không tồn tại.");

                if (parent.Level >= 3)
                {
                    throw new ValidationException("Không thể tạo danh mục mới - danh mục cha đã ở cấp 3 (tối đa 3 cấp).");
                }
            }

            if (string.Equals(normalizedSlug, "tree", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Slug \"tree\" là từ khoá bảo lưu của hệ thống, vui lòng chọn slug khác.");

            if (await _db.Categories.AnyAsync(x => x.Slug.ToLower() == normalizedSlug, ct))
                throw new ValidationException($"Slug \"{normalizedSlug}\" đã tồn tại, vui lòng chọn slug khác.");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name?.Trim() ?? string.Empty,
                Slug = normalizedSlug,
                ParentId = dto.ParentId,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow,
            };

            _db.Categories.Add(category);
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                if (await _db.Categories.AnyAsync(x => x.Slug.ToLower() == normalizedSlug, ct))
                    throw new ValidationException($"Slug \"{normalizedSlug}\" đã tồn tại, vui lòng chọn slug khác.");
                throw;
            }

            await _db.Categories.Entry(category).ReloadAsync(ct);
            string? parentName = null;
            if (category.ParentId.HasValue)
            {
                parentName = await _db.Categories.Where(x => x.Id == category.ParentId.Value).Select(x => x.Name).FirstOrDefaultAsync(ct);
            }
            return ToDto(category, parentName);
        }

        public async Task UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default)
        {
            if (dto.SortOrder < 1)
                throw new ValidationException("Thứ tự sắp xếp phải lớn hơn hoặc bằng 1.");

            var category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new ValidationException("Danh mục không tồn tại.");

            var normalizedSlug = dto.Slug?.Trim().ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedSlug))
                throw new ValidationException("Slug không được để trống.");

            if (string.Equals(normalizedSlug, "tree", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Slug \"tree\" là từ khoá bảo lưu của hệ thống, vui lòng chọn slug khác.");

            if (await _db.Categories.AnyAsync(x => x.Slug.ToLower() == normalizedSlug && x.Id != id, ct))
                throw new ValidationException($"Slug \"{normalizedSlug}\" đã tồn tại, vui lòng chọn slug khác.");

            category.Name = dto.Name?.Trim() ?? string.Empty;
            category.Slug = normalizedSlug;
            category.SortOrder = dto.SortOrder;

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                if (await _db.Categories.AnyAsync(x => x.Slug.ToLower() == normalizedSlug && x.Id != id, ct))
                    throw new ValidationException($"Slug \"{normalizedSlug}\" đã tồn tại, vui lòng chọn slug khác.");
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new ValidationException("Danh mục không tồn tại.");

            if (await _db.Categories.AnyAsync(x => x.ParentId == id, ct))
                throw new ValidationException("Không thể xoá danh mục đang có danh mục con.");

            if (await _db.Posts.AnyAsync(x => x.CategoryId == id, ct))
                throw new ValidationException("Không thể xoá danh mục đang có bài viết thuộc về danh mục này.");

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync(ct);
        }
    }
}
