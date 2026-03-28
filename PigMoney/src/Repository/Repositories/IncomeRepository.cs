//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Repositories;

using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

public class IncomeRepository(AppDbContext context) : Repository<Income>(context), IIncomeRepository
{
    public override async Task<Result<Income>> GetByIdAsync(int id)
    {
        Income? entity = await DbSet
            .Include(x => x.Category)
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return Result<Income>.Failure($"Income with id {id} not found");
        }

        return Result<Income>.Success(entity);
    }

    public async Task<Result<IEnumerable<Income>>> GetFilteredAsync(
        DateTime? startDate,
        DateTime? endDate,
        int? categoryId,
        int? accountId,
        int page,
        int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 50;
        }

        IQueryable<Income> query = DbSet
            .Include(x => x.Category)
            .Include(x => x.Account);

        if (startDate.HasValue)
        {
            query = query.Where(x => x.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.Date <= endDate.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        if (accountId.HasValue)
        {
            query = query.Where(x => x.AccountId == accountId.Value);
        }

        List<Income> entities = await query
            .OrderByDescending(x => x.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<IEnumerable<Income>>.Success(entities);
    }

    public async Task<Result<int>> GetTotalCountAsync(
        DateTime? startDate,
        DateTime? endDate,
        int? categoryId,
        int? accountId)
    {
        IQueryable<Income> query = DbSet;

        if (startDate.HasValue)
        {
            query = query.Where(x => x.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.Date <= endDate.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        if (accountId.HasValue)
        {
            query = query.Where(x => x.AccountId == accountId.Value);
        }

        int count = await query.CountAsync();

        return Result<int>.Success(count);
    }
}
