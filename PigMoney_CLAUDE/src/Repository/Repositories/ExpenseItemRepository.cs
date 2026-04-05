//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

namespace Repository.Repositories;

public class ExpenseItemRepository(AppDbContext context) : Repository<ExpenseItem>(context), IExpenseItemRepository
{
    public async Task<IEnumerable<ExpenseItem>> GetByExpenseIdAsync(int expenseId, int page, int pageSize)
    {
        return await DbSet
            .Where(ei => ei.ExpenseId == expenseId)
            .OrderBy(ei => ei.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountByExpenseIdAsync(int expenseId)
    {
        return await DbSet.CountAsync(ei => ei.ExpenseId == expenseId);
    }
}
