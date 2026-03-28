//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace E2ETests;

using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Common;
using Application.DTOs.Expenses;
using Domain.Enums;

public class EnhancedPerformanceTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task GetEndpoints_100Requests_AverageWithin300ms()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "PerfGet", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "PerfAcc", AccountType.Checking, 5000m);
        var expense = await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id, 100m, "PerfExpense");

        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            var sw = Stopwatch.StartNew();
            var response = await client.GetAsync($"/api/v1/expenses/{expense.Id}");
            sw.Stop();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            times.Add(sw.ElapsedMilliseconds);
        }

        var average = times.Average();
        Assert.True(average < 300, $"Average GET response time {average}ms exceeds 300ms threshold");
    }

    [Fact]
    public async Task PostEndpoints_100Requests_AverageWithin300ms()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "PerfPost", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "PerfPostAcc", AccountType.Checking, 10000m);

        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            var request = new CreateExpenseRequest(10m + i, DateTime.UtcNow, category.Id, account.Id, $"Perf {i}", null);
            var sw = Stopwatch.StartNew();
            var response = await client.PostAsJsonAsync("/api/v1/expenses", request);
            sw.Stop();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            times.Add(sw.ElapsedMilliseconds);
        }

        var average = times.Average();
        Assert.True(average < 300, $"Average POST response time {average}ms exceeds 300ms threshold");
    }

    [Fact]
    public async Task ConcurrentGetRequests_100Simultaneous_AllSucceed()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "Conc100Get", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "Conc100Acc", AccountType.Checking, 5000m);
        var expense = await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id, 100m, "Concurrent");

        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(client.GetAsync($"/api/v1/expenses/{expense.Id}"));
        }

        var responses = await Task.WhenAll(tasks);

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
    }

    [Fact]
    public async Task ConcurrentPostRequests_100Simultaneous_AllSucceed()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "Conc100Post", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "Conc100PostAcc", AccountType.Checking, 50000m);

        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 100; i++)
        {
            var request = new CreateExpenseRequest(10m + i, DateTime.UtcNow, category.Id, account.Id, $"Concurrent {i}", null);
            tasks.Add(client.PostAsJsonAsync("/api/v1/expenses", request));
        }

        var responses = await Task.WhenAll(tasks);

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
    }

    [Fact]
    public async Task LoadTest_Query1000Expenses_Within300ms()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "Load1000", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "Load1000Acc", AccountType.Checking, 100000m);

        await TestDataSeeder.SeedMultipleExpensesAsync(factory, category.Id, account.Id, 1000);

        var sw = Stopwatch.StartNew();
        var response = await client.GetAsync($"/api/v1/expenses?categoryId={category.Id}&page=1&pageSize=50");
        sw.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(sw.ElapsedMilliseconds < 300, $"Query with 1000+ expenses took {sw.ElapsedMilliseconds}ms, expected <300ms");
    }

    [Fact]
    public async Task LoadTest_FilterByDateRange_1000Expenses_Within300ms()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "LoadDate", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "LoadDateAcc", AccountType.Checking, 100000m);

        await TestDataSeeder.SeedMultipleExpensesAsync(factory, category.Id, account.Id, 1000);

        var startDate = DateTime.UtcNow.AddDays(-500).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.AddDays(-100).ToString("yyyy-MM-dd");

        var sw = Stopwatch.StartNew();
        var response = await client.GetAsync($"/api/v1/expenses?startDate={startDate}&endDate={endDate}&page=1&pageSize=50");
        sw.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(sw.ElapsedMilliseconds < 300, $"Date filter query took {sw.ElapsedMilliseconds}ms, expected <300ms");
    }

    [Fact]
    public async Task Pagination_150Expenses_Page2Size50_ReturnsCorrectPage()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "Pag150", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "Pag150Acc", AccountType.Cash, 0m);

        await TestDataSeeder.SeedMultipleExpensesAsync(factory, category.Id, account.Id, 150);

        var response = await client.GetAsync($"/api/v1/expenses?categoryId={category.Id}&page=2&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ExpenseResponse>>>(TestHelpers.JsonOptions);
        Assert.NotNull(wrapper);
        var result = wrapper.Data!;
        Assert.Equal(2, result.Page);
        Assert.Equal(50, result.PageSize);
        Assert.Equal(50, result.Items.Count);
        Assert.Equal(150, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }
}
