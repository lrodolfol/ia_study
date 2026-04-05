//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Accounts;
using Application.DTOs.Common;
using Application.DTOs.Incomes;
using Domain.Enums;

namespace pigMoney.Tests.Integration;

public class AccountsIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public AccountsIntegrationTests(TestWebApplicationFactory factory)
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

    [Fact]
    public async Task PostAccount_ShouldReturn201WithLocationHeader()
    {
        var request = new CreateAccountRequest("Test Account", AccountType.Checking, 1000m);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/accounts", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        await AssertEnvelopeAsync(response, 201);

        AccountResponse? created = await ReadDataAsync<AccountResponse>(response);
        Assert.NotNull(created);
        Assert.Equal("Test Account", created.Name);
        Assert.Equal(AccountType.Checking, created.Type);
        Assert.Equal(1000m, created.Balance);
    }

    [Fact]
    public async Task PostAccount_WithEmptyName_ShouldReturn400()
    {
        var request = new CreateAccountRequest("", AccountType.Checking, 100m);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/accounts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertEnvelopeAsync(response, 400);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement errorArray = doc.RootElement.GetProperty("error");
        Assert.True(errorArray.GetArrayLength() > 0);
    }

    [Fact]
    public async Task PostAccount_WithNegativeBalance_ShouldReturn400()
    {
        var request = new CreateAccountRequest("Negative", AccountType.Cash, -100m);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/accounts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountById_ShouldReturn200()
    {
        var request = new CreateAccountRequest("ById Account", AccountType.Savings, 500m);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/accounts", request);
        AccountResponse? created = await ReadDataAsync<AccountResponse>(createResponse);

        HttpResponseMessage response = await _client.GetAsync($"/api/v1/accounts/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        AccountResponse? account = await ReadDataAsync<AccountResponse>(response);
        Assert.NotNull(account);
        Assert.Equal(created.Id, account.Id);
    }

    [Fact]
    public async Task GetAccountById_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/accounts/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertEnvelopeAsync(response, 404);
    }

    [Fact]
    public async Task GetAccounts_ShouldReturn200WithPagination()
    {
        var request = new CreateAccountRequest("Listed Account", AccountType.Savings, 500m);
        await _client.PostAsJsonAsync("/api/v1/accounts", request);

        HttpResponseMessage response = await _client.GetAsync("/api/v1/accounts?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        PaginatedList<AccountResponse>? result = await ReadDataAsync<PaginatedList<AccountResponse>>(response);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task PutAccount_ShouldReturn200()
    {
        var createRequest = new CreateAccountRequest("Original", AccountType.Cash, 100m);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/accounts", createRequest);
        AccountResponse? created = await ReadDataAsync<AccountResponse>(createResponse);

        var updateRequest = new UpdateAccountRequest("Updated", AccountType.Credit, 200m);
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/accounts/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        AccountResponse? updated = await ReadDataAsync<AccountResponse>(response);
        Assert.Equal("Updated", updated!.Name);
        Assert.Equal(AccountType.Credit, updated.Type);
    }

    [Fact]
    public async Task PutAccount_WhenNotFound_ShouldReturn404()
    {
        var request = new UpdateAccountRequest("Ghost", AccountType.Cash, 0m);
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/accounts/9999", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_WhenNoDependents_ShouldReturn204()
    {
        var createRequest = new CreateAccountRequest("ToDelete", AccountType.Cash, 0m);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/accounts", createRequest);
        AccountResponse? created = await ReadDataAsync<AccountResponse>(createResponse);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/accounts/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.DeleteAsync("/api/v1/accounts/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_WithLinkedIncomes_ShouldReturn409()
    {
        var accountRequest = new CreateAccountRequest("WithIncome", AccountType.Checking, 5000m);
        HttpResponseMessage accountResponse = await _client.PostAsJsonAsync("/api/v1/accounts", accountRequest);
        AccountResponse? account = await ReadDataAsync<AccountResponse>(accountResponse);

        var incomeRequest = new CreateIncomeRequest(1000m, DateTime.UtcNow, "Salary", account!.Id);
        await _client.PostAsJsonAsync("/api/v1/incomes", incomeRequest);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/accounts/{account.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertEnvelopeAsync(response, 409);
    }
}
