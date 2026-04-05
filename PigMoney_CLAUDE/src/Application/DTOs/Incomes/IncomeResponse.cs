//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.Incomes;

public record IncomeResponse(int Id, decimal Amount, DateTime Date, string Description, int AccountId, DateTime CreatedAt, DateTime UpdatedAt);
