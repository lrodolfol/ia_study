//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Entities;

namespace Domain.Interfaces;

public interface IIncomeRepository : IRepository<Income>
{
    Task<IEnumerable<Income>> GetFilteredAsync(DateTime? startDate, DateTime? endDate, int? accountId, int page, int pageSize);
    Task<int> CountFilteredAsync(DateTime? startDate, DateTime? endDate, int? accountId);
}
