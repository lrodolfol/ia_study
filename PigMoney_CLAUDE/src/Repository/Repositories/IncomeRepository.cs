//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

namespace Repository.Repositories;

public class IncomeRepository(AppDbContext context) : Repository<Income>(context), IIncomeRepository
{
    public async Task<IEnumerable<Income>> GetFilteredAsync(DateTime? startDate, DateTime? endDate, int? accountId, int page, int pageSize)
    {
        IQueryable<Income> query = DbSet.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(i => i.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(i => i.Date <= endDate.Value);

        if (accountId.HasValue)
            query = query.Where(i => i.AccountId == accountId.Value);

        return await query
            .OrderBy(i => i.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountFilteredAsync(DateTime? startDate, DateTime? endDate, int? accountId)
    {
        IQueryable<Income> query = DbSet.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(i => i.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(i => i.Date <= endDate.Value);

        if (accountId.HasValue)
            query = query.Where(i => i.AccountId == accountId.Value);

        return await query.CountAsync();
    }
}
