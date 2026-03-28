//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Repository.Tests;

using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Repositories;

public class ExpenseItemRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<Expense> SetupExpenseAsync(AppDbContext context)
    {
        var category = new Category { Name = "Test Category", Type = TransactionType.Expense };
        var account = new Account { Name = "Test Account", Type = AccountType.Checking, InitialBalance = 1000 };
        await context.Categories.AddAsync(category);
        await context.Accounts.AddAsync(account);
        await context.SaveChangesAsync();

        var expense = new Expense
        {
            Amount = 500,
            Date = DateTime.UtcNow,
            CategoryId = category.Id,
            AccountId = account.Id,
            Description = "Test Expense"
        };
        await context.Expenses.AddAsync(expense);
        await context.SaveChangesAsync();

        return expense;
    }

    [Fact]
    public async Task GetByExpenseIdAsync_ReturnsItemsForExpense()
    {
        using AppDbContext context = CreateContext();
        Expense expense = await SetupExpenseAsync(context);
        var repository = new ExpenseItemRepository(context);

        await context.ExpenseItems.AddAsync(new ExpenseItem { ExpenseId = expense.Id, Amount = 100, Description = "Item 1" });
        await context.ExpenseItems.AddAsync(new ExpenseItem { ExpenseId = expense.Id, Amount = 200, Description = "Item 2" });
        await context.SaveChangesAsync();

        var result = await repository.GetByExpenseIdAsync(expense.Id, 1, 50);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count());
    }

    [Fact]
    public async Task GetSumByExpenseIdAsync_ReturnsCorrectSum()
    {
        using AppDbContext context = CreateContext();
        Expense expense = await SetupExpenseAsync(context);
        var repository = new ExpenseItemRepository(context);

        await context.ExpenseItems.AddAsync(new ExpenseItem { ExpenseId = expense.Id, Amount = 100, Description = "Item 1" });
        await context.ExpenseItems.AddAsync(new ExpenseItem { ExpenseId = expense.Id, Amount = 150, Description = "Item 2" });
        await context.ExpenseItems.AddAsync(new ExpenseItem { ExpenseId = expense.Id, Amount = 50, Description = "Item 3" });
        await context.SaveChangesAsync();

        var result = await repository.GetSumByExpenseIdAsync(expense.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(300, result.Value);
    }

    [Fact]
    public async Task GetTotalCountByExpenseAsync_ReturnsCorrectCount()
    {
        using AppDbContext context = CreateContext();
        Expense expense = await SetupExpenseAsync(context);
        var repository = new ExpenseItemRepository(context);

        await context.ExpenseItems.AddAsync(new ExpenseItem { ExpenseId = expense.Id, Amount = 100, Description = "Item 1" });
        await context.ExpenseItems.AddAsync(new ExpenseItem { ExpenseId = expense.Id, Amount = 200, Description = "Item 2" });
        await context.ExpenseItems.AddAsync(new ExpenseItem { ExpenseId = expense.Id, Amount = 300, Description = "Item 3", IsDeleted = true });
        await context.SaveChangesAsync();

        var result = await repository.GetTotalCountByExpenseAsync(expense.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task GetByIdAsync_IncludesExpenseAndCategory()
    {
        using AppDbContext context = CreateContext();
        Expense expense = await SetupExpenseAsync(context);
        var repository = new ExpenseItemRepository(context);

        var expenseItem = new ExpenseItem
        {
            ExpenseId = expense.Id,
            Amount = 100,
            Description = "Test Item"
        };
        await context.ExpenseItems.AddAsync(expenseItem);
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(expenseItem.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Expense);
        Assert.Equal("Test Expense", result.Value.Expense.Description);
    }
}
