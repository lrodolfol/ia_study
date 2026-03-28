//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.ExpenseItems;

public record ExpenseItemResponse(
    int Id,
    int ExpenseId,
    decimal Amount,
    string Description,
    int? CategoryId,
    string? CategoryName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
