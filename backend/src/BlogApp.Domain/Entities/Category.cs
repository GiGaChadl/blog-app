namespace BlogApp.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 1 = cha, 2 = con, 3 = cháu. TỰ TÍNH bởi trigger DB (fn_set_category_level) —
    /// không set tay giá trị này trong code Application, chỉ đọc để hiển thị/kiểm tra.
    /// </summary>
    public short Level { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
