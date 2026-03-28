//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.Expenses;

public record ExpenseFilterParams(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? CategoryId = null,
    int? AccountId = null,
    int Page = 1,
    int PageSize = 50
);
