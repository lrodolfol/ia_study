//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

namespace Repository.Repositories;

public class IncomeRepository(AppDbContext context) : Repository<Income>(context), IIncomeRepository
{
    public async Task<IEnumerable<Income>> GetFilteredAsync(IncomeFilterParams filters, int page, int pageSize)
    {
        return await ApplyFilters(filters)
            .OrderBy(i => i.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountFilteredAsync(IncomeFilterParams filters)
    {
        return await ApplyFilters(filters).CountAsync();
    }

    private IQueryable<Income> ApplyFilters(IncomeFilterParams filters)
    {
        IQueryable<Income> query = DbSet.AsQueryable();

        if (filters.StartDate.HasValue)
            query = query.Where(i => i.Date >= filters.StartDate.Value);

        if (filters.EndDate.HasValue)
            query = query.Where(i => i.Date <= filters.EndDate.Value);

        if (filters.AccountId.HasValue)
            query = query.Where(i => i.AccountId == filters.AccountId.Value);

        return query;
    }
}
