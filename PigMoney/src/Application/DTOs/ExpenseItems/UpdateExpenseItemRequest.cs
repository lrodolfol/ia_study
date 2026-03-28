//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.ExpenseItems;

public record UpdateExpenseItemRequest(
    decimal? Amount,
    string? Description,
    int? CategoryId
);
