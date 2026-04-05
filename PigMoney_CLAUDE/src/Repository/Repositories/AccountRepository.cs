//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

namespace Repository.Repositories;

public class AccountRepository(AppDbContext context) : Repository<Account>(context), IAccountRepository
{
    public new async Task<Account?> GetByIdAsync(int id) =>
        await DbSet
            .Include(a => a.Incomes)
            .Include(a => a.Expenses)
            .FirstOrDefaultAsync(a => a.Id == id);
}
