//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Interfaces;

using Domain.Common;
using Domain.Entities;

public interface IExpenseItemRepository : IRepository<ExpenseItem>
{
    Task<Result<IEnumerable<ExpenseItem>>> GetByExpenseIdAsync(int expenseId, int page, int pageSize);
    Task<Result<decimal>> GetSumByExpenseIdAsync(int expenseId);
    Task<Result<int>> GetTotalCountByExpenseAsync(int expenseId);
}
