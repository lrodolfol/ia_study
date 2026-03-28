//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Entities;

public class ExpenseItem() : BaseEntity
{
    public int ExpenseId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }

    public Expense Expense { get; set; } = null!;
    public Category? Category { get; set; }
}
