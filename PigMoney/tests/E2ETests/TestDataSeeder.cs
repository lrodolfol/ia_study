//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace E2ETests;

using Domain.Entities;
using Domain.Enums;
using Repository.Data;

public static class TestDataSeeder
{
    public static async Task<Category> SeedCategoryAsync(TestWebApplicationFactory factory, string name = "Food", TransactionType type = TransactionType.Expense)
    {
        return await factory.SeedAsync(async db =>
        {
            var category = new Category
            {
                Name = name,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return category;
        });
    }

    public static async Task<Account> SeedAccountAsync(TestWebApplicationFactory factory, string name = "Checking", AccountType type = AccountType.Checking, decimal initialBalance = 1000m)
    {
        return await factory.SeedAsync(async db =>
        {
            var account = new Account
            {
                Name = name,
                Type = type,
                InitialBalance = initialBalance,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
            return account;
        });
    }

    public static async Task<Expense> SeedExpenseAsync(
        TestWebApplicationFactory factory,
        int categoryId,
        int accountId,
        decimal amount = 100m,
        string description = "Test Expense",
        DateTime? date = null)
    {
        return await factory.SeedAsync(async db =>
        {
            var expense = new Expense
            {
                Amount = amount,
                Date = date ?? DateTime.UtcNow,
                CategoryId = categoryId,
                AccountId = accountId,
                Description = description,
                Notes = "Test notes",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            db.Expenses.Add(expense);
            await db.SaveChangesAsync();
            return expense;
        });
    }

    public static async Task<Income> SeedIncomeAsync(
        TestWebApplicationFactory factory,
        int categoryId,
        int accountId,
        decimal amount = 5000m,
        string description = "Test Income",
        DateTime? date = null)
    {
        return await factory.SeedAsync(async db =>
        {
            var income = new Income
            {
                Amount = amount,
                Date = date ?? DateTime.UtcNow,
                CategoryId = categoryId,
                AccountId = accountId,
                Description = description,
                Notes = "Test notes",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            db.Incomes.Add(income);
            await db.SaveChangesAsync();
            return income;
        });
    }

    public static async Task<Budget> SeedBudgetAsync(
        TestWebApplicationFactory factory,
        int categoryId,
        decimal limitAmount = 500m,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        return await factory.SeedAsync(async db =>
        {
            var budget = new Budget
            {
                CategoryId = categoryId,
                LimitAmount = limitAmount,
                StartDate = startDate ?? new DateTime(2024, 1, 1),
                EndDate = endDate ?? new DateTime(2024, 1, 31),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            db.Budgets.Add(budget);
            await db.SaveChangesAsync();
            return budget;
        });
    }

    public static async Task<ExpenseItem> SeedExpenseItemAsync(
        TestWebApplicationFactory factory,
        int expenseId,
        decimal amount = 25m,
        string description = "Test Item",
        int? categoryId = null)
    {
        return await factory.SeedAsync(async db =>
        {
            var item = new ExpenseItem
            {
                ExpenseId = expenseId,
                Amount = amount,
                Description = description,
                CategoryId = categoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            db.ExpenseItems.Add(item);
            await db.SaveChangesAsync();
            return item;
        });
    }

    public static async Task SeedMultipleExpensesAsync(TestWebApplicationFactory factory, int categoryId, int accountId, int count)
    {
        await factory.SeedAsync(async db =>
        {
            for (int i = 0; i < count; i++)
            {
                var expense = new Expense
                {
                    Amount = 10m + i,
                    Date = DateTime.UtcNow.AddDays(-i),
                    CategoryId = categoryId,
                    AccountId = accountId,
                    Description = $"Expense {i + 1}",
                    Notes = $"Notes for expense {i + 1}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                db.Expenses.Add(expense);
            }
            
            await db.SaveChangesAsync();
        });
    }
}
