//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

namespace Repository.Repositories;

public class ExpenseRepository(AppDbContext context) : Repository<Expense>(context), IExpenseRepository
{
    public async Task<IEnumerable<Expense>> GetFilteredAsync(ExpenseFilterParams filters, int page, int pageSize)
    {
        return await ApplyFilters(filters)
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountFilteredAsync(ExpenseFilterParams filters)
    {
        return await ApplyFilters(filters).CountAsync();
    }

    private IQueryable<Expense> ApplyFilters(ExpenseFilterParams filters)
    {
        IQueryable<Expense> query = DbSet.AsQueryable();

        if (filters.StartDate.HasValue)
            query = query.Where(e => e.Date >= filters.StartDate.Value);

        if (filters.EndDate.HasValue)
            query = query.Where(e => e.Date <= filters.EndDate.Value);

        if (filters.AccountId.HasValue)
            query = query.Where(e => e.AccountId == filters.AccountId.Value);

        if (filters.CategoryId.HasValue)
            query = query.Where(e => e.CategoryId == filters.CategoryId.Value);

        return query;
    }
}
