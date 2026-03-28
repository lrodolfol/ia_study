//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Tests;

using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Repositories;

public class CategoryRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task HasDependenciesAsync_ReturnsTrue_WhenCategoryHasExpenses()
    {
        using AppDbContext context = CreateContext();
        var category = new Category { Name = "Test Category", Type = TransactionType.Expense };
        var account = new Account { Name = "Test Account", Type = AccountType.Checking };
        await context.Categories.AddAsync(category);
        await context.Accounts.AddAsync(account);
        await context.SaveChangesAsync();

        await context.Expenses.AddAsync(new Expense
        {
            Amount = 100,
            Date = DateTime.UtcNow,
            CategoryId = category.Id,
            AccountId = account.Id
        });
        await context.SaveChangesAsync();

        var repository = new CategoryRepository(context);
        var result = await repository.HasDependenciesAsync(category.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task HasDependenciesAsync_ReturnsFalse_WhenCategoryHasNoDependencies()
    {
        using AppDbContext context = CreateContext();
        var category = new Category { Name = "Test Category", Type = TransactionType.Expense };
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        var repository = new CategoryRepository(context);
        var result = await repository.HasDependenciesAsync(category.Id);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectCount()
    {
        using AppDbContext context = CreateContext();
        await context.Categories.AddAsync(new Category { Name = "Category 1", Type = TransactionType.Expense });
        await context.Categories.AddAsync(new Category { Name = "Category 2", Type = TransactionType.Income });
        await context.Categories.AddAsync(new Category { Name = "Category 3", Type = TransactionType.Expense, IsDeleted = true });
        await context.SaveChangesAsync();

        var repository = new CategoryRepository(context);
        var result = await repository.GetTotalCountAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
    }
}
