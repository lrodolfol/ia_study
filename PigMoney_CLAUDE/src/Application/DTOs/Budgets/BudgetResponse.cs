//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.Budgets;

public record BudgetResponse(int Id, int CategoryId, DateTime StartDate, DateTime EndDate, decimal LimitAmount, DateTime CreatedAt, DateTime UpdatedAt);
