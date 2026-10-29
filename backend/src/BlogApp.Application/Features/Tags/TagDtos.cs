namespace BlogApp.Application.Features.Tags;

public record TagDto(Guid Id, string Name, string Slug, int PostCount);

public record CreateTagDto(string Name, string Slug);

public record UpdateTagDto(string Name, string Slug);
