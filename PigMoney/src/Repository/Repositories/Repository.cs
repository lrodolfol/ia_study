//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Repositories;

using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

public class Repository<T>(AppDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<Result<T>> GetByIdAsync(int id)
    {
        T? entity = await DbSet.FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return Result<T>.Failure($"{typeof(T).Name} with id {id} not found");
        }

        return Result<T>.Success(entity);
    }

    public virtual async Task<Result<IEnumerable<T>>> GetAllAsync(int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 50;
        }

        List<T> entities = await DbSet
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<IEnumerable<T>>.Success(entities);
    }

    public virtual async Task<Result<T>> AddAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await DbSet.AddAsync(entity);

        return Result<T>.Success(entity);
    }

    public virtual async Task<Result<T>> UpdateAsync(T entity)
    {
        T? existing = await DbSet.FirstOrDefaultAsync(x => x.Id == entity.Id);

        if (existing is null)
        {
            return Result<T>.Failure($"{typeof(T).Name} with id {entity.Id} not found");
        }

        entity.UpdatedAt = DateTime.UtcNow;
        Context.Entry(existing).CurrentValues.SetValues(entity);

        return Result<T>.Success(entity);
    }

    public virtual async Task<Result> DeleteAsync(int id)
    {
        T? entity = await DbSet.FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return Result.Failure($"{typeof(T).Name} with id {id} not found");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public virtual async Task<Result> SaveChangesAsync()
    {
        await Context.SaveChangesAsync();
        return Result.Success();
    }
}
