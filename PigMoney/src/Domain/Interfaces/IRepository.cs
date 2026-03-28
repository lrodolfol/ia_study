//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Interfaces;

using Domain.Common;
using System.Linq.Expressions;

public interface IRepository<T> where T : IAggregateRoot
{
    Task<Result<T>> GetByIdAsync(int id);
    Task<Result<IEnumerable<T>>> GetAllAsync(int page, int pageSize, Expression<Func<T, object>> orderBy);
    Task<Result<T>> AddAsync(T entity);
    Task<Result<T>> UpdateAsync(T entity);
    Task<Result> DeleteAsync(int id);
    Task<Result> SaveChangesAsync();
}
