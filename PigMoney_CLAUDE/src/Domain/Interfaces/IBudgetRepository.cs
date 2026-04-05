//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Common;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<IEnumerable<Budget>> GetFilteredAsync(BudgetFilterParams filters, int page, int pageSize);
    Task<int> CountFilteredAsync(BudgetFilterParams filters);
}
