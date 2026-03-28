//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Entities;

using Domain.Enums;

public class Category() : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }

    public List<Expense> Expenses { get; set; } = [];
    public List<Income> Incomes { get; set; } = [];
    public List<Budget> Budgets { get; set; } = [];
}
