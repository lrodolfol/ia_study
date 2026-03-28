//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Repositories;

using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

public class BudgetRepository(AppDbContext context) : Repository<Budget>(context), IBudgetRepository
{
    public override async Task<Result<Budget>> GetByIdAsync(int id)
    {
        Budget? entity = await DbSet
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return Result<Budget>.Failure($"Budget with id {id} not found");
        }

        return Result<Budget>.Success(entity);
    }

    public async Task<Result<IEnumerable<Budget>>> GetByCategoryIdAsync(int categoryId, int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 50;
        }

        List<Budget> entities = await DbSet
            .Include(x => x.Category)
            .Where(x => x.CategoryId == categoryId)
            .OrderByDescending(x => x.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<IEnumerable<Budget>>.Success(entities);
    }

    public async Task<Result<int>> GetTotalCountAsync()
    {
        int count = await DbSet.CountAsync();
        return Result<int>.Success(count);
    }

    public async Task<Result<int>> GetTotalCountByCategoryAsync(int categoryId)
    {
        int count = await DbSet.Where(x => x.CategoryId == categoryId).CountAsync();
        return Result<int>.Success(count);
    }
}
