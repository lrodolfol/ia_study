//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Repositories;

using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

public class ExpenseRepository(AppDbContext context) : Repository<Expense>(context), IExpenseRepository
{
    public override async Task<Result<Expense>> GetByIdAsync(int id)
    {
        Expense? entity = await DbSet
            .Include(x => x.Category)
            .Include(x => x.Account)
            .Include(x => x.ExpenseItems)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return Result<Expense>.Failure($"Expense with id {id} not found");
        }

        return Result<Expense>.Success(entity);
    }

    public async Task<Result<IEnumerable<Expense>>> GetFilteredAsync(
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

        IQueryable<Expense> query = DbSet
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

        List<Expense> entities = await query
            .OrderByDescending(x => x.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<IEnumerable<Expense>>.Success(entities);
    }

    public async Task<Result<int>> GetTotalCountAsync(
        DateTime? startDate,
        DateTime? endDate,
        int? categoryId,
        int? accountId)
    {
        IQueryable<Expense> query = DbSet;

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
