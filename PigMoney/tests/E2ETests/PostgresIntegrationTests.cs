//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace E2ETests;

using System.Net;
using System.Net.Http.Json;
using Application.DTOs.ExpenseItems;
using Application.DTOs.Common;
using Application.DTOs.Expenses;
using Domain.Entities;
using Domain.Enums;

[Collection("PostgresTests")]
public class PostgresIntegrationTests : IAsyncLifetime
{
    private readonly PostgresTestWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public PostgresIntegrationTests()
    {
        _factory = new PostgresTestWebApplicationFactory();
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task PostgreSQL_CreateExpense_PersistsCorrectly()
    {
        var category = await SeedCategoryAsync("PGExpCat", TransactionType.Expense);
        var account = await SeedAccountAsync("PGExpAcc", AccountType.Checking, 1000m);

        var request = new CreateExpenseRequest(75.50m, DateTime.UtcNow, category.Id, account.Id, "PostgreSQL Test", "Notes");
        var response = await _client.PostAsJsonAsync("/api/v1/expenses", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<ExpenseResponse>>(TestHelpers.JsonOptions);
        Assert.NotNull(wrapper?.Data);
        Assert.Equal(75.50m, wrapper.Data.Amount);
    }

    [Fact]
    public async Task PostgreSQL_SoftDelete_GlobalFilterExcludesDeleted()
    {
        var category = await SeedCategoryAsync("SoftDelCat", TransactionType.Expense);
        var account = await SeedAccountAsync("SoftDelAcc", AccountType.Checking, 500m);

        var request = new CreateExpenseRequest(25m, DateTime.UtcNow, category.Id, account.Id, "ToDelete", null);
        var createResponse = await _client.PostAsJsonAsync("/api/v1/expenses", request);
        var createWrapper = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ExpenseResponse>>(TestHelpers.JsonOptions);
        var createdId = createWrapper!.Data!.Id;

        await _client.DeleteAsync($"/api/v1/expenses/{createdId}");

        var getResponse = await _client.GetAsync($"/api/v1/expenses/{createdId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var listResponse = await _client.GetAsync($"/api/v1/expenses?categoryId={category.Id}&page=1&pageSize=100");
        var listWrapper = await listResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ExpenseResponse>>>(TestHelpers.JsonOptions);
        Assert.DoesNotContain(listWrapper!.Data!.Items, e => e.Id == createdId);
    }

    [Fact]
    public async Task PostgreSQL_ExpenseItemsSum_ApplicationValidation()
    {
        var category = await SeedCategoryAsync("ItemSumCat", TransactionType.Expense);
        var account = await SeedAccountAsync("ItemSumAcc", AccountType.Checking, 200m);
        var expense = await SeedExpenseAsync(category.Id, account.Id, 100m, "ParentExpense");

        var item1 = new CreateExpenseItemRequest(expense.Id, 60m, "Item1", null);
        var response1 = await _client.PostAsJsonAsync("/api/v1/expense-items", item1);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

        var item2 = new CreateExpenseItemRequest(expense.Id, 50m, "Item2 Exceeds", null);
        var response2 = await _client.PostAsJsonAsync("/api/v1/expense-items", item2);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }

    [Fact]
    public async Task PostgreSQL_ConcurrentInserts_AllSucceed()
    {
        var category = await SeedCategoryAsync("ConcPGCat", TransactionType.Expense);
        var account = await SeedAccountAsync("ConcPGAcc", AccountType.Checking, 10000m);

        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 50; i++)
        {
            var request = new CreateExpenseRequest(10m + i, DateTime.UtcNow, category.Id, account.Id, $"PG Concurrent {i}", null);
            tasks.Add(_client.PostAsJsonAsync("/api/v1/expenses", request));
        }

        var responses = await Task.WhenAll(tasks);
        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
    }

    [Fact]
    public async Task PostgreSQL_LargeDataset_QueryPerformance()
    {
        var category = await SeedCategoryAsync("PGLargeCat", TransactionType.Expense);
        var account = await SeedAccountAsync("PGLargeAcc", AccountType.Checking, 50000m);

        await SeedMultipleExpensesAsync(category.Id, account.Id, 500);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/v1/expenses?categoryId={category.Id}&page=1&pageSize=50");
        sw.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(sw.ElapsedMilliseconds < 500, $"PostgreSQL query took {sw.ElapsedMilliseconds}ms");
    }

    private async Task<Category> SeedCategoryAsync(string name, TransactionType type)
    {
        return await _factory.SeedAsync(async db =>
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

    private async Task<Account> SeedAccountAsync(string name, AccountType type, decimal initialBalance)
    {
        return await _factory.SeedAsync(async db =>
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

    private async Task<Expense> SeedExpenseAsync(int categoryId, int accountId, decimal amount, string description)
    {
        return await _factory.SeedAsync(async db =>
        {
            var expense = new Expense
            {
                Amount = amount,
                Date = DateTime.UtcNow,
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

    private async Task SeedMultipleExpensesAsync(int categoryId, int accountId, int count)
    {
        await _factory.SeedAsync(async db =>
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
