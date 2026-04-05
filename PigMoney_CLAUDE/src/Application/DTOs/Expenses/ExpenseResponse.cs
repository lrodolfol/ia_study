//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.Expenses;

public record ExpenseResponse(int Id, decimal Amount, DateTime Date, string Description, int AccountId, int CategoryId, DateTime CreatedAt, DateTime UpdatedAt);
