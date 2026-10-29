namespace BlogApp.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public Guid MediaId { get; set; }
    public string Title { get; set; } = null!;
    public string? ShortDescription { get; set; }
    public string? Tag { get; set; }
    public int SortOrder { get; set; }

    public MediaLibrary Media { get; set; } = null!;
}
