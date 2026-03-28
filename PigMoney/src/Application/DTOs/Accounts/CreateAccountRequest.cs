//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.Accounts;

using Domain.Enums;

public record CreateAccountRequest(
    string Name,
    AccountType Type,
    decimal InitialBalance
);
