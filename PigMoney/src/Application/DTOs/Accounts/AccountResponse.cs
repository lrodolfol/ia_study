//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.Accounts;

using Domain.Enums;

public record AccountResponse(
    int Id,
    string Name,
    AccountType Type,
    decimal InitialBalance,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
