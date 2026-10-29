using BlogApp.Domain.Enums;

namespace BlogApp.Domain.Entities;

/// <summary>Dùng Google AdSense thật — AdsenseSlotId là ad unit Google cấp, không tự upload ảnh.</summary>
public class Ad
{
    public Guid Id { get; set; }
    public AdPosition Position { get; set; } // unique — mỗi vị trí 1 cấu hình
    public string AdsenseSlotId { get; set; } = null!;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
