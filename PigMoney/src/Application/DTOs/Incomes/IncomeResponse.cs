//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.Incomes;

public record IncomeResponse(
    int Id,
    decimal Amount,
    DateTime Date,
    int CategoryId,
    string CategoryName,
    int AccountId,
    string AccountName,
    string Description,
    string Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
