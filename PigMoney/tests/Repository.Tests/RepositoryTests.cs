//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Tests;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Repositories;

public class RepositoryTests
{
    private static AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsSuccess_WhenEntityFound()
    {
        using AppDbContext context = CreateContext();
        var repository = new CategoryRepository(context);
        var category = new Category { Name = "Test Category" };
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(category.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Test Category", result.Value!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsFailure_WhenEntityNotFound()
    {
        using AppDbContext context = CreateContext();
        var repository = new CategoryRepository(context);

        var result = await repository.GetByIdAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_ExcludesSoftDeletedRecords()
    {
        using AppDbContext context = CreateContext();
        var repository = new CategoryRepository(context);
        var category = new Category { Name = "Deleted Category", IsDeleted = true };
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(category.Id);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        using AppDbContext context = CreateContext();
        var repository = new CategoryRepository(context);

        for (int i = 0; i < 10; i++)
        {
            await context.Categories.AddAsync(new Category { Name = $"Category {i}" });
        }
        await context.SaveChangesAsync();

        var result = await repository.GetAllAsync(1, 5, (x => x.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.Count());
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedRecords()
    {
        using AppDbContext context = CreateContext();
        var repository = new CategoryRepository(context);

        await context.Categories.AddAsync(new Category { Name = "Active Category" });
        await context.Categories.AddAsync(new Category { Name = "Deleted Category", IsDeleted = true });
        await context.SaveChangesAsync();

        var result = await repository.GetAllAsync(1, 50, (x => x.Id));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Active Category", result.Value!.First().Name);
    }

    [Fact]
    public async Task AddAsync_CreatesNewEntity_AndReturnsSuccess()
    {
        using AppDbContext context = CreateContext();
        var repository = new CategoryRepository(context);
        var category = new Category { Name = "New Category" };

        var result = await repository.AddAsync(category);
        await repository.SaveChangesAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("New Category", result.Value!.Name);
        Assert.NotEqual(default, result.Value.CreatedAt);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingEntity_AndReturnsSuccess()
    {
        using AppDbContext context = CreateContext();
        var repository = new CategoryRepository(context);
        var category = new Category { Name = "Original Name" };
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        category.Name = "Updated Name";
        var result = await repository.UpdateAsync(category);
        await repository.SaveChangesAsync();

        Assert.True(result.IsSuccess);

        Category? updated = await context.Categories.FindAsync(category.Id);
        Assert.Equal("Updated Name", updated!.Name);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesEntity_SetsIsDeletedTrue()
    {
        using AppDbContext context = CreateContext();
        var repository = new CategoryRepository(context);
        var category = new Category { Name = "To Delete" };
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        var result = await repository.DeleteAsync(category.Id);
        await repository.SaveChangesAsync();

        Assert.True(result.IsSuccess);

        Category? deleted = await context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == category.Id);
        Assert.True(deleted!.IsDeleted);
    }
}
