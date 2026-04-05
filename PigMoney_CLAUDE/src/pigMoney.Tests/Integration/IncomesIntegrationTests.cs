//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Accounts;
using Application.DTOs.Common;
using Application.DTOs.Incomes;
using Domain.Enums;

namespace pigMoney.Tests.Integration;

public class IncomesIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IncomesIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<AccountResponse> CreateTestAccountAsync()
    {
        var request = new CreateAccountRequest("Test Account", AccountType.Checking, 5000m);
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/accounts", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountResponse>())!;
    }

    [Fact]
    public async Task PostIncome_ShouldReturn201()
    {
        AccountResponse account = await CreateTestAccountAsync();
        var request = new CreateIncomeRequest(1500m, DateTime.UtcNow, "Monthly salary", account.Id);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/incomes", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        IncomeResponse? created = await response.Content.ReadFromJsonAsync<IncomeResponse>();
        Assert.NotNull(created);
        Assert.Equal(1500m, created.Amount);
        Assert.Equal("Monthly salary", created.Description);
        Assert.Equal(account.Id, created.AccountId);
    }

    [Fact]
    public async Task GetIncomes_WithFilters_ShouldReturn200WithPagination()
    {
        AccountResponse account = await CreateTestAccountAsync();
        var request = new CreateIncomeRequest(1000m, DateTime.UtcNow, "Filtered income", account.Id);
        await _client.PostAsJsonAsync("/api/v1/incomes", request);

        HttpResponseMessage response = await _client.GetAsync(
            $"/api/v1/incomes?page=1&pageSize=10&accountId={account.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PaginatedList<IncomeResponse>? result = await response.Content.ReadFromJsonAsync<PaginatedList<IncomeResponse>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetIncomeById_ShouldReturn200()
    {
        AccountResponse account = await CreateTestAccountAsync();
        var request = new CreateIncomeRequest(500m, DateTime.UtcNow, "Test income", account.Id);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/incomes", request);
        IncomeResponse? created = await createResponse.Content.ReadFromJsonAsync<IncomeResponse>();

        HttpResponseMessage response = await _client.GetAsync($"/api/v1/incomes/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        IncomeResponse? income = await response.Content.ReadFromJsonAsync<IncomeResponse>();
        Assert.NotNull(income);
        Assert.Equal(created.Id, income.Id);
    }

    [Fact]
    public async Task GetIncomeById_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/incomes/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutIncome_ShouldReturn200()
    {
        AccountResponse account = await CreateTestAccountAsync();
        var createRequest = new CreateIncomeRequest(1000m, DateTime.UtcNow, "Original", account.Id);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/incomes", createRequest);
        IncomeResponse? created = await createResponse.Content.ReadFromJsonAsync<IncomeResponse>();

        var updateRequest = new UpdateIncomeRequest(2000m, DateTime.UtcNow, "Updated", account.Id);
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/incomes/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        IncomeResponse? updated = await response.Content.ReadFromJsonAsync<IncomeResponse>();
        Assert.Equal(2000m, updated!.Amount);
        Assert.Equal("Updated", updated.Description);
    }

    [Fact]
    public async Task DeleteIncome_ShouldReturn204()
    {
        AccountResponse account = await CreateTestAccountAsync();
        var createRequest = new CreateIncomeRequest(300m, DateTime.UtcNow, "ToDelete", account.Id);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/incomes", createRequest);
        IncomeResponse? created = await createResponse.Content.ReadFromJsonAsync<IncomeResponse>();

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/incomes/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
