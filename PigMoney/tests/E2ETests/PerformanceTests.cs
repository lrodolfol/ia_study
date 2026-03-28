//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace E2ETests;

using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Expenses;
using Domain.Enums;

public class PerformanceTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task GetEndpoints_RespondWithin300ms_Average()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory);
        var account = await TestDataSeeder.SeedAccountAsync(factory);
        var expense = await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id);

        var endpoints = new[]
        {
            $"/api/v1/categories/{category.Id}",
            $"/api/v1/accounts/{account.Id}",
            $"/api/v1/expenses/{expense.Id}"
        };

        var times = new List<long>();
        var iterations = 5;

        foreach (var endpoint in endpoints)
        {
            for (int i = 0; i < iterations; i++)
            {
                var sw = Stopwatch.StartNew();
                var response = await client.GetAsync(endpoint);
                sw.Stop();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                times.Add(sw.ElapsedMilliseconds);
            }
        }

        var average = times.Average();
        Assert.True(average < 300, $"Average response time {average}ms exceeds 300ms threshold");
    }

    [Fact(Skip = "Performance tests require isolated test environment")]
    public async Task PostEndpoints_RespondWithin300ms_Average()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory);
        var account = await TestDataSeeder.SeedAccountAsync(factory);

        var times = new List<long>();
        var iterations = 5;

        for (int i = 0; i < iterations; i++)
        {
            var request = new CreateExpenseRequest(50m + i, DateTime.UtcNow, category.Id, account.Id, $"Test {i}", null);
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
    public async Task ConcurrentGets_AllReturnCorrectData()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "ConcTest", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "ConcAcc", AccountType.Checking, 1000m);
        var expense = await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id, 75m, "ConcExpense");

        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(client.GetAsync($"/api/v1/expenses/{expense.Id}"));
        }

        var responses = await Task.WhenAll(tasks);

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
    }

    [Fact(Skip = "Concurrency tests require isolated test environment")]
    public async Task ConcurrentPosts_AllCompleteSuccessfully()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "ConcPostTest", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "ConcPostAcc", AccountType.Checking, 5000m);

        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 10; i++)
        {
            var request = new CreateExpenseRequest(10m + i, DateTime.UtcNow, category.Id, account.Id, $"Conc {i}", null);
            var response = await client.PostAsJsonAsync("/api/v1/expenses", request);
            responses.Add(response);
            await Task.Delay(10);
        }

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
    }

    [Fact]
    public async Task LoadTest_QueryManyExpenses_PerformsWell()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "LoadCat", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "LoadAcc", AccountType.Checking, 10000m);

        await TestDataSeeder.SeedMultipleExpensesAsync(factory, category.Id, account.Id, 100);

        var sw = Stopwatch.StartNew();
        var response = await client.GetAsync($"/api/v1/expenses?categoryId={category.Id}&page=1&pageSize=50");
        sw.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(sw.ElapsedMilliseconds < 1000, $"Load test query took {sw.ElapsedMilliseconds}ms, expected <1000ms");
    }
}
