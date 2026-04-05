//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Common;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IEnumerable<Expense>> GetFilteredAsync(ExpenseFilterParams filters, int page, int pageSize);
    Task<int> CountFilteredAsync(ExpenseFilterParams filters);
}
