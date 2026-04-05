//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.Incomes;

public record UpdateIncomeRequest(decimal Amount, DateTime Date, string Description, int AccountId);
