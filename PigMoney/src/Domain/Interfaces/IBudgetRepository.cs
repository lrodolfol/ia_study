//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Interfaces;

using Domain.Common;
using Domain.Entities;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<Result<IEnumerable<Budget>>> GetByCategoryIdAsync(int categoryId, int page, int pageSize);
    Task<Result<int>> GetTotalCountAsync();
    Task<Result<int>> GetTotalCountByCategoryAsync(int categoryId);
}
