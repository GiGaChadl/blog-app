namespace BlogApp.Application.Features.Administration.Contact;

public record SocialLinkDto(
    string Platform,
    string Url,
    string? Icon = null
);

public record ContactInfoDto(
    string Email,
    string? Location,
    List<SocialLinkDto> SocialLinks,
    DateTime UpdatedAt,
    string? Address = null,
    string? Phone = null,
    string? Bio = null,
    string? About = null,
    string? Copyright = null,
    string? FacebookUrl = null,
    string? GithubUrl = null,
    string? LinkedinUrl = null,
    string? YoutubeUrl = null,
    string? AuthorName = null,
    string? Slogan = null
);

public record UpdateContactInfoDto(
    string Email,
    string? Location = null,
    List<SocialLinkDto>? SocialLinks = null,
    string? Address = null,
    string? Phone = null,
    string? Bio = null,
    string? About = null,
    string? Copyright = null,
    string? FacebookUrl = null,
    string? GithubUrl = null,
    string? LinkedinUrl = null,
    string? YoutubeUrl = null,
    string? AuthorName = null,
    string? Slogan = null
)
{
    public UpdateContactInfoDto() : this(string.Empty) { }
}


