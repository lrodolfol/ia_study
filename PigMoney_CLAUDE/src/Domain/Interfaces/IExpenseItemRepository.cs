//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Entities;

namespace Domain.Interfaces;

public interface IExpenseItemRepository : IRepository<ExpenseItem>
{
    Task<IEnumerable<ExpenseItem>> GetByExpenseIdAsync(int expenseId, int page, int pageSize);
    Task<int> CountByExpenseIdAsync(int expenseId);
}
