//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Entities;

using Domain.Enums;

public class Account() : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal InitialBalance { get; set; }

    public List<Expense> Expenses { get; set; } = [];
    public List<Income> Incomes { get; set; } = [];
}
