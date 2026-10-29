using System;
using System.Collections.Generic;
using System.Text;

namespace BlogApp.Application.Features.Categories
{
    public record CategoryDto(Guid Id, string Name, string Slug, Guid? ParentId, short Level, int SortOrder, string? ParentName = null);
    // record khác class ở chỗ 2 đối tượng tạo ra từ đối tượng này thì giống nhau nếu nội dung giống nhau.
    // ví dụ: var a = CategoryDto("Category1");
    //        var b = CategoryDto("Category1");
    //        => a = b;
    // class thì a != b;
    public record CategoryTreeDto(Guid Id, string Name, string Slug, short Level, List<CategoryTreeDto> Children);

    public record CreateCategoryDto(string Name, string Slug, Guid? ParentId, int SortOrder = 1);

    public record UpdateCategoryDto(string Name, string Slug, int SortOrder = 1);
}
