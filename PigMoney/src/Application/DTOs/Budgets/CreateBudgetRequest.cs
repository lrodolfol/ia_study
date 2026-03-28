//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.Budgets;

public record CreateBudgetRequest(
    int CategoryId,
    decimal LimitAmount,
    DateTime StartDate,
    DateTime EndDate
);
