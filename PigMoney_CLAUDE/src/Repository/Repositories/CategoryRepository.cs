//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

namespace Repository.Repositories;

public class CategoryRepository(AppDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public new async Task<Category?> GetByIdAsync(int id) =>
        await DbSet
            .Include(c => c.Expenses)
            .Include(c => c.Budgets)
            .FirstOrDefaultAsync(c => c.Id == id);
}
