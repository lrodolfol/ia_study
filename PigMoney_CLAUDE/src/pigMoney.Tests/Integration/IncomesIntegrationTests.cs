//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Accounts;
using Application.DTOs.Common;
using Application.DTOs.Incomes;
using Domain.Enums;

namespace pigMoney.Tests.Integration;

public class IncomesIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public IncomesIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static async Task<T?> ReadDataAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement dataElement = doc.RootElement.GetProperty("data");
        return JsonSerializer.Deserialize<T>(dataElement.GetRawText(), JsonOptions);
    }

    private static async Task AssertEnvelopeAsync(HttpResponseMessage response, int expectedStatusCode)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        Assert.True(root.TryGetProperty("statusCode", out JsonElement statusCodeProp));
        Assert.Equal(expectedStatusCode, statusCodeProp.GetInt32());
        Assert.True(root.TryGetProperty("message", out _));
        Assert.True(root.TryGetProperty("error", out _));
        Assert.True(root.TryGetProperty("data", out _));
    }

    private async Task<AccountResponse> CreateTestAccountAsync()
    {
        var request = new CreateAccountRequest("Income Account", AccountType.Checking, 5000m);
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/accounts", request);
        response.EnsureSuccessStatusCode();
        return (await ReadDataAsync<AccountResponse>(response))!;
    }

    [Fact]
    public async Task PostIncome_ShouldReturn201WithLocationHeader()
    {
        AccountResponse account = await CreateTestAccountAsync();
        var request = new CreateIncomeRequest(1500m, DateTime.UtcNow, "Monthly salary", account.Id);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/incomes", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        await AssertEnvelopeAsync(response, 201);

        IncomeResponse? created = await ReadDataAsync<IncomeResponse>(response);
        Assert.NotNull(created);
        Assert.Equal(1500m, created.Amount);
        Assert.Equal("Monthly salary", created.Description);
        Assert.Equal(account.Id, created.AccountId);
    }

    [Fact]
    public async Task PostIncome_WithZeroAmount_ShouldReturn400()
    {
        AccountResponse account = await CreateTestAccountAsync();
        var request = new CreateIncomeRequest(0m, DateTime.UtcNow, "Invalid", account.Id);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/incomes", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertEnvelopeAsync(response, 400);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement errorArray = doc.RootElement.GetProperty("error");
        Assert.True(errorArray.GetArrayLength() > 0);
    }

    [Fact]
    public async Task PostIncome_WithEmptyDescription_ShouldReturn400()
    {
        AccountResponse account = await CreateTestAccountAsync();
        var request = new CreateIncomeRequest(100m, DateTime.UtcNow, "", account.Id);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/incomes", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
        await AssertEnvelopeAsync(response, 200);

        PaginatedList<IncomeResponse>? result = await ReadDataAsync<PaginatedList<IncomeResponse>>(response);
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
        IncomeResponse? created = await ReadDataAsync<IncomeResponse>(createResponse);

        HttpResponseMessage response = await _client.GetAsync($"/api/v1/incomes/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        IncomeResponse? income = await ReadDataAsync<IncomeResponse>(response);
        Assert.NotNull(income);
        Assert.Equal(created.Id, income.Id);
    }

    [Fact]
    public async Task GetIncomeById_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/incomes/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertEnvelopeAsync(response, 404);
    }

    [Fact]
    public async Task PutIncome_ShouldReturn200()
    {
        AccountResponse account = await CreateTestAccountAsync();
        var createRequest = new CreateIncomeRequest(1000m, DateTime.UtcNow, "Original", account.Id);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/incomes", createRequest);
        IncomeResponse? created = await ReadDataAsync<IncomeResponse>(createResponse);

        var updateRequest = new UpdateIncomeRequest(2000m, DateTime.UtcNow, "Updated", account.Id);
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/incomes/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        IncomeResponse? updated = await ReadDataAsync<IncomeResponse>(response);
        Assert.Equal(2000m, updated!.Amount);
        Assert.Equal("Updated", updated.Description);
    }

    [Fact]
    public async Task PutIncome_WhenNotFound_ShouldReturn404()
    {
        AccountResponse account = await CreateTestAccountAsync();
        var request = new UpdateIncomeRequest(100m, DateTime.UtcNow, "Ghost", account.Id);
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/incomes/9999", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteIncome_ShouldReturn204()
    {
        AccountResponse account = await CreateTestAccountAsync();
        var createRequest = new CreateIncomeRequest(300m, DateTime.UtcNow, "ToDelete", account.Id);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/incomes", createRequest);
        IncomeResponse? created = await ReadDataAsync<IncomeResponse>(createResponse);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/incomes/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteIncome_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.DeleteAsync("/api/v1/incomes/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
