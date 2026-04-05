//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Enums;

namespace Application.DTOs.Accounts;

public record AccountResponse(int Id, string Name, AccountType Type, decimal Balance, DateTime CreatedAt, DateTime UpdatedAt);
