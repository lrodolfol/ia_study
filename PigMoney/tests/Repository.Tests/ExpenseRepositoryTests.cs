//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Tests;

using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Repositories;

public class ExpenseRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<(Category category, Account account)> SetupDependenciesAsync(AppDbContext context)
    {
        var category = new Category { Name = "Test Category", Type = TransactionType.Expense };
        var account = new Account { Name = "Test Account", Type = AccountType.Checking, InitialBalance = 1000 };

        await context.Categories.AddAsync(category);
        await context.Accounts.AddAsync(account);
        await context.SaveChangesAsync();

        return (category, account);
    }

    [Fact]
    public async Task GetByIdAsync_IncludesNavigationProperties()
    {
        using AppDbContext context = CreateContext();
        var (category, account) = await SetupDependenciesAsync(context);
        var repository = new ExpenseRepository(context);

        var expense = new Expense
        {
            Amount = 100,
            Date = DateTime.UtcNow,
            CategoryId = category.Id,
            AccountId = account.Id,
            Description = "Test Expense"
        };
        await context.Expenses.AddAsync(expense);
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(expense.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Test Category", result.Value!.Category.Name);
        Assert.Equal("Test Account", result.Value.Account.Name);
    }

    [Fact]
    public async Task GetFilteredAsync_FiltersByDateRange()
    {
        using AppDbContext context = CreateContext();
        var (category, account) = await SetupDependenciesAsync(context);
        var repository = new ExpenseRepository(context);

        DateTime baseDate = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        await context.Expenses.AddAsync(new Expense { Amount = 100, Date = baseDate.AddDays(-10), CategoryId = category.Id, AccountId = account.Id });
        await context.Expenses.AddAsync(new Expense { Amount = 200, Date = baseDate, CategoryId = category.Id, AccountId = account.Id });
        await context.Expenses.AddAsync(new Expense { Amount = 300, Date = baseDate.AddDays(10), CategoryId = category.Id, AccountId = account.Id });
        await context.SaveChangesAsync();

        var result = await repository.GetFilteredAsync(baseDate.AddDays(-5), baseDate.AddDays(5), null, null, 1, 50);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(200, result.Value!.First().Amount);
    }

    [Fact]
    public async Task GetFilteredAsync_FiltersByCategoryId()
    {
        using AppDbContext context = CreateContext();
        var (category1, account) = await SetupDependenciesAsync(context);
        var category2 = new Category { Name = "Other Category", Type = TransactionType.Expense };
        await context.Categories.AddAsync(category2);
        await context.SaveChangesAsync();

        var repository = new ExpenseRepository(context);

        await context.Expenses.AddAsync(new Expense { Amount = 100, Date = DateTime.UtcNow, CategoryId = category1.Id, AccountId = account.Id });
        await context.Expenses.AddAsync(new Expense { Amount = 200, Date = DateTime.UtcNow, CategoryId = category2.Id, AccountId = account.Id });
        await context.SaveChangesAsync();

        var result = await repository.GetFilteredAsync(null, null, category1.Id, null, 1, 50);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(100, result.Value!.First().Amount);
    }

    [Fact]
    public async Task GetFilteredAsync_FiltersByAccountId()
    {
        using AppDbContext context = CreateContext();
        var (category, account1) = await SetupDependenciesAsync(context);
        var account2 = new Account { Name = "Other Account", Type = AccountType.Savings, InitialBalance = 500 };
        await context.Accounts.AddAsync(account2);
        await context.SaveChangesAsync();

        var repository = new ExpenseRepository(context);

        await context.Expenses.AddAsync(new Expense { Amount = 100, Date = DateTime.UtcNow, CategoryId = category.Id, AccountId = account1.Id });
        await context.Expenses.AddAsync(new Expense { Amount = 200, Date = DateTime.UtcNow, CategoryId = category.Id, AccountId = account2.Id });
        await context.SaveChangesAsync();

        var result = await repository.GetFilteredAsync(null, null, null, account2.Id, 1, 50);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(200, result.Value!.First().Amount);
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectCount()
    {
        using AppDbContext context = CreateContext();
        var (category, account) = await SetupDependenciesAsync(context);
        var repository = new ExpenseRepository(context);

        await context.Expenses.AddAsync(new Expense { Amount = 100, Date = DateTime.UtcNow, CategoryId = category.Id, AccountId = account.Id });
        await context.Expenses.AddAsync(new Expense { Amount = 200, Date = DateTime.UtcNow, CategoryId = category.Id, AccountId = account.Id });
        await context.Expenses.AddAsync(new Expense { Amount = 300, Date = DateTime.UtcNow, CategoryId = category.Id, AccountId = account.Id, IsDeleted = true });
        await context.SaveChangesAsync();

        var result = await repository.GetTotalCountAsync(null, null, null, null);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
    }
}
