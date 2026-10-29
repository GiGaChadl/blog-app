namespace BlogApp.Application.Common.Models;

public record PagedResult<T>(List<T> Items, int Total, int Page, int PageSize);
