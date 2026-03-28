//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Repositories;

using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

public class AccountRepository(AppDbContext context) : Repository<Account>(context), IAccountRepository
{
    public async Task<Result<bool>> HasDependenciesAsync(int id)
    {
        bool hasExpenses = await Context.Expenses.AnyAsync(x => x.AccountId == id);
        bool hasIncomes = await Context.Incomes.AnyAsync(x => x.AccountId == id);

        return Result<bool>.Success(hasExpenses || hasIncomes);
    }

    public async Task<Result<int>> GetTotalCountAsync()
    {
        int count = await DbSet.CountAsync();
        return Result<int>.Success(count);
    }
}
