//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.Budgets;

public record BudgetResponse(
    int Id,
    int CategoryId,
    string CategoryName,
    decimal LimitAmount,
    DateTime StartDate,
    DateTime EndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
