//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.Common;

public record PaginatedList<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);
