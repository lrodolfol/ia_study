//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Common;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IIncomeRepository : IRepository<Income>
{
    Task<IEnumerable<Income>> GetFilteredAsync(IncomeFilterParams filters, int page, int pageSize);
    Task<int> CountFilteredAsync(IncomeFilterParams filters);
}
