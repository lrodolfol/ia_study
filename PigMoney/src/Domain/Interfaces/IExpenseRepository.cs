//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Interfaces;

using Domain.Common;
using Domain.Entities;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<Result<IEnumerable<Expense>>> GetFilteredAsync(
        DateTime? startDate,
        DateTime? endDate,
        int? categoryId,
        int? accountId,
        int page,
        int pageSize);

    Task<Result<int>> GetTotalCountAsync(
        DateTime? startDate,
        DateTime? endDate,
        int? categoryId,
        int? accountId);
}
