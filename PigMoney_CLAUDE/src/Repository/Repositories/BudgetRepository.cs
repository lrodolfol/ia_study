//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

namespace Repository.Repositories;

public class BudgetRepository(AppDbContext context) : Repository<Budget>(context), IBudgetRepository
{
    public async Task<IEnumerable<Budget>> GetFilteredAsync(BudgetFilterParams filters, int page, int pageSize)
    {
        return await ApplyFilters(filters)
            .OrderBy(b => b.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountFilteredAsync(BudgetFilterParams filters)
    {
        return await ApplyFilters(filters).CountAsync();
    }

    private IQueryable<Budget> ApplyFilters(BudgetFilterParams filters)
    {
        IQueryable<Budget> query = DbSet.AsQueryable();

        if (filters.CategoryId.HasValue)
            query = query.Where(b => b.CategoryId == filters.CategoryId.Value);

        if (filters.StartDate.HasValue)
            query = query.Where(b => b.StartDate >= filters.StartDate.Value);

        if (filters.EndDate.HasValue)
            query = query.Where(b => b.EndDate <= filters.EndDate.Value);

        return query;
    }
}
