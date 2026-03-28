//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace E2ETests;

using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Common;
using Application.DTOs.Expenses;
using Application.DTOs.Incomes;
using Application.DTOs.Budgets;
using Domain.Enums;

public class FilterAndPaginationTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task FilterExpenses_ByDateRange_ReturnsCorrectSubset()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory);
        var account = await TestDataSeeder.SeedAccountAsync(factory);

        var now = DateTime.UtcNow;
        await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id, 10m, "Old", now.AddDays(-10));
        await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id, 20m, "InRange1", now.AddDays(-5));
        await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id, 30m, "InRange2", now.AddDays(-3));

        var startDate = now.AddDays(-6).ToString("yyyy-MM-dd");
        var endDate = now.AddDays(-2).ToString("yyyy-MM-dd");
        var response = await client.GetAsync($"/api/v1/expenses?startDate={startDate}&endDate={endDate}&page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FilterExpenses_ByCategory_ReturnsOnlyMatching()
    {
        using var client = factory.CreateClient();
        var category1 = await TestDataSeeder.SeedCategoryAsync(factory, "CatA", TransactionType.Expense);
        var category2 = await TestDataSeeder.SeedCategoryAsync(factory, "CatB", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory);

        await TestDataSeeder.SeedExpenseAsync(factory, category1.Id, account.Id, 100m, "Cat1-1");
        await TestDataSeeder.SeedExpenseAsync(factory, category1.Id, account.Id, 200m, "Cat1-2");
        await TestDataSeeder.SeedExpenseAsync(factory, category2.Id, account.Id, 300m, "Cat2-1");

        var response = await client.GetAsync($"/api/v1/expenses?categoryId={category1.Id}&page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ExpenseResponse>>>(TestHelpers.JsonOptions);
        Assert.NotNull(wrapper);
        var result = wrapper.Data!;
        Assert.True(result.Items.All(e => e.CategoryId == category1.Id));
    }

    [Fact]
    public async Task FilterExpenses_ByAccount_ReturnsOnlyMatching()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory);
        var account1 = await TestDataSeeder.SeedAccountAsync(factory, "Acc1", AccountType.Checking, 1000m);
        var account2 = await TestDataSeeder.SeedAccountAsync(factory, "Acc2", AccountType.Savings, 2000m);

        await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account1.Id, 100m, "Acc1-1");
        await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account1.Id, 200m, "Acc1-2");
        await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account2.Id, 300m, "Acc2-1");

        var response = await client.GetAsync($"/api/v1/expenses?accountId={account1.Id}&page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ExpenseResponse>>>(TestHelpers.JsonOptions);
        Assert.NotNull(wrapper);
        var result = wrapper.Data!;
        Assert.True(result.Items.All(e => e.AccountId == account1.Id));
    }

    [Fact]
    public async Task FilterIncomes_ByDateRange_ReturnsCorrectSubset()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "IncomeCategory", TransactionType.Income);
        var account = await TestDataSeeder.SeedAccountAsync(factory);

        var now = DateTime.UtcNow;
        await TestDataSeeder.SeedIncomeAsync(factory, category.Id, account.Id, 1000m, "OldIncome", now.AddDays(-10));
        await TestDataSeeder.SeedIncomeAsync(factory, category.Id, account.Id, 2000m, "InRange1", now.AddDays(-5));
        await TestDataSeeder.SeedIncomeAsync(factory, category.Id, account.Id, 3000m, "InRange2", now.AddDays(-3));

        var startDate = now.AddDays(-6).ToString("yyyy-MM-dd");
        var endDate = now.AddDays(-2).ToString("yyyy-MM-dd");
        var response = await client.GetAsync($"/api/v1/incomes?startDate={startDate}&endDate={endDate}&page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FilterBudgets_ByCategory_ReturnsOnlyMatching()
    {
        using var client = factory.CreateClient();
        var category1 = await TestDataSeeder.SeedCategoryAsync(factory, "BudgetCat1", TransactionType.Expense);
        var category2 = await TestDataSeeder.SeedCategoryAsync(factory, "BudgetCat2", TransactionType.Expense);

        await TestDataSeeder.SeedBudgetAsync(factory, category1.Id, 500m);
        await TestDataSeeder.SeedBudgetAsync(factory, category2.Id, 1000m);

        var response = await client.GetAsync($"/api/v1/budgets/by-category/{category1.Id}?page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<BudgetResponse>>>(TestHelpers.JsonOptions);
        Assert.NotNull(wrapper);
        var result = wrapper.Data!;
        Assert.True(result.Items.All(b => b.CategoryId == category1.Id));
    }

    [Fact]
    public async Task Pagination_Page2_ReturnsCorrectItems()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "PagTest", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "PagAcc", AccountType.Cash, 0m);

        await TestDataSeeder.SeedMultipleExpensesAsync(factory, category.Id, account.Id, 25);

        var response = await client.GetAsync($"/api/v1/expenses?categoryId={category.Id}&page=2&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ExpenseResponse>>>(TestHelpers.JsonOptions);
        Assert.NotNull(wrapper);
        var result = wrapper.Data!;
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task Pagination_AllEndpoints_ReturnCorrectMetadata()
    {
        using var client = factory.CreateClient();
        
        var categoriesResponse = await client.GetAsync("/api/v1/categories?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, categoriesResponse.StatusCode);

        var accountsResponse = await client.GetAsync("/api/v1/accounts?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, accountsResponse.StatusCode);

        var budgetsResponse = await client.GetAsync("/api/v1/budgets?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, budgetsResponse.StatusCode);

        var incomesResponse = await client.GetAsync("/api/v1/incomes?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, incomesResponse.StatusCode);
    }
}
