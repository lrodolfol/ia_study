//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Repositories;

using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

public class ExpenseItemRepository(AppDbContext context) : Repository<ExpenseItem>(context), IExpenseItemRepository
{
    public override async Task<Result<ExpenseItem>> GetByIdAsync(int id)
    {
        ExpenseItem? entity = await DbSet
            .Include(x => x.Expense)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return Result<ExpenseItem>.Failure($"ExpenseItem with id {id} not found");
        }

        return Result<ExpenseItem>.Success(entity);
    }

    public async Task<Result<IEnumerable<ExpenseItem>>> GetByExpenseIdAsync(int expenseId, int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 50;
        }

        List<ExpenseItem> entities = await DbSet
            .Include(x => x.Category)
            .Where(x => x.ExpenseId == expenseId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<IEnumerable<ExpenseItem>>.Success(entities);
    }

    public async Task<Result<decimal>> GetSumByExpenseIdAsync(int expenseId)
    {
        decimal sum = await DbSet
            .Where(x => x.ExpenseId == expenseId)
            .SumAsync(x => x.Amount);

        return Result<decimal>.Success(sum);
    }

    public async Task<Result<int>> GetTotalCountByExpenseAsync(int expenseId)
    {
        int count = await DbSet.Where(x => x.ExpenseId == expenseId).CountAsync();
        return Result<int>.Success(count);
    }
}
