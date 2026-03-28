//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Interfaces;

using Domain.Common;
using Domain.Entities;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Result<bool>> HasDependenciesAsync(int id);
    Task<Result<int>> GetTotalCountAsync();
}
