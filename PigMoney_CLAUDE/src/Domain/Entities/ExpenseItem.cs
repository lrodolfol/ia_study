//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Common;

namespace Domain.Entities;

public class ExpenseItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int ExpenseId { get; set; }
    public Expense Expense { get; set; } = null!;
}
