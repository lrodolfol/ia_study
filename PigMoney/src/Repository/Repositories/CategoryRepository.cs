//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Repositories;

using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

public class CategoryRepository(AppDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public async Task<Result<bool>> HasDependenciesAsync(int id)
    {
        bool hasExpenses = await Context.Expenses.AnyAsync(x => x.CategoryId == id);
        bool hasIncomes = await Context.Incomes.AnyAsync(x => x.CategoryId == id);
        bool hasBudgets = await Context.Budgets.AnyAsync(x => x.CategoryId == id);

        return Result<bool>.Success(hasExpenses || hasIncomes || hasBudgets);
    }

    public async Task<Result<int>> GetTotalCountAsync()
    {
        int count = await DbSet.CountAsync();
        return Result<int>.Success(count);
    }
}
