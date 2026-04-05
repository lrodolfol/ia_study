//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Accounts;
using Application.DTOs.Common;
using Domain.Enums;

namespace pigMoney.Tests.Integration;

public class AccountsIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AccountsIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostAccount_ShouldReturn201()
    {
        var request = new CreateAccountRequest("Test Account", AccountType.Checking, 1000m);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/accounts", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        AccountResponse? created = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(created);
        Assert.Equal("Test Account", created.Name);
    }

    [Fact]
    public async Task GetAccountById_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/accounts/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAccounts_ShouldReturn200WithPagination()
    {
        var request = new CreateAccountRequest("Listed Account", AccountType.Savings, 500m);
        await _client.PostAsJsonAsync("/api/v1/accounts", request);

        HttpResponseMessage response = await _client.GetAsync("/api/v1/accounts?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PaginatedList<AccountResponse>? result = await response.Content.ReadFromJsonAsync<PaginatedList<AccountResponse>>();
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
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        AccountResponse? created = await createResponse.Content.ReadFromJsonAsync<AccountResponse>();

        var updateRequest = new UpdateAccountRequest("Updated", AccountType.Credit, 200m);
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/accounts/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AccountResponse? updated = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.Equal("Updated", updated!.Name);
    }

    [Fact]
    public async Task DeleteAccount_WhenNoDependents_ShouldReturn204()
    {
        var createRequest = new CreateAccountRequest("ToDelete", AccountType.Cash, 0m);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/accounts", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        AccountResponse? created = await createResponse.Content.ReadFromJsonAsync<AccountResponse>();

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/accounts/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
