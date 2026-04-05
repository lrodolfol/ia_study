//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Accounts;
using Application.DTOs.Budgets;
using Application.DTOs.Categories;
using Application.DTOs.Common;
using Application.DTOs.Expenses;
using Domain.Enums;

namespace pigMoney.Tests.Integration;

public class CategoriesIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public CategoriesIntegrationTests(TestWebApplicationFactory factory)
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
    public async Task PostCategory_ShouldReturn201WithLocationHeader()
    {
        var request = new CreateCategoryRequest("Food", "Groceries");

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/categories", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        await AssertEnvelopeAsync(response, 201);

        CategoryResponse? created = await ReadDataAsync<CategoryResponse>(response);
        Assert.NotNull(created);
        Assert.Equal("Food", created.Name);
    }

    [Fact]
    public async Task PostCategory_WithEmptyName_ShouldReturn400()
    {
        var request = new CreateCategoryRequest("", "No name");

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/categories", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertEnvelopeAsync(response, 400);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement errorArray = doc.RootElement.GetProperty("error");
        Assert.True(errorArray.GetArrayLength() > 0);
    }

    [Fact]
    public async Task GetCategoryById_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/categories/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertEnvelopeAsync(response, 404);
    }

    [Fact]
    public async Task GetCategories_ShouldReturn200WithPagination()
    {
        var request = new CreateCategoryRequest("Transport", "Bus and metro");
        await _client.PostAsJsonAsync("/api/v1/categories", request);

        HttpResponseMessage response = await _client.GetAsync("/api/v1/categories?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        PaginatedList<CategoryResponse>? result = await ReadDataAsync<PaginatedList<CategoryResponse>>(response);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task PutCategory_ShouldReturn200()
    {
        var createRequest = new CreateCategoryRequest("Original", "Old desc");
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/categories", createRequest);
        CategoryResponse? created = await ReadDataAsync<CategoryResponse>(createResponse);

        var updateRequest = new UpdateCategoryRequest("Updated", "New desc");
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/categories/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        CategoryResponse? updated = await ReadDataAsync<CategoryResponse>(response);
        Assert.Equal("Updated", updated!.Name);
    }

    [Fact]
    public async Task PutCategory_WhenNotFound_ShouldReturn404()
    {
        var request = new UpdateCategoryRequest("Ghost", "Desc");
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/categories/9999", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WhenNoDependents_ShouldReturn204()
    {
        var createRequest = new CreateCategoryRequest("ToDelete", "Remove me");
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/categories", createRequest);
        CategoryResponse? created = await ReadDataAsync<CategoryResponse>(createResponse);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/categories/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.DeleteAsync("/api/v1/categories/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WithLinkedExpenses_ShouldReturn409()
    {
        var categoryRequest = new CreateCategoryRequest("WithExpense", "Has expenses");
        HttpResponseMessage catResponse = await _client.PostAsJsonAsync("/api/v1/categories", categoryRequest);
        CategoryResponse? category = await ReadDataAsync<CategoryResponse>(catResponse);

        var accountRequest = new CreateAccountRequest("ExpAccount", AccountType.Cash, 5000m);
        HttpResponseMessage accResponse = await _client.PostAsJsonAsync("/api/v1/accounts", accountRequest);
        AccountResponse? account = await ReadDataAsync<AccountResponse>(accResponse);

        var expenseRequest = new CreateExpenseRequest(100m, DateTime.UtcNow, "Test expense", account!.Id, category!.Id);
        await _client.PostAsJsonAsync("/api/v1/expenses", expenseRequest);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/categories/{category.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertEnvelopeAsync(response, 409);
    }

    [Fact]
    public async Task DeleteCategory_WithLinkedBudgets_ShouldReturn409()
    {
        var categoryRequest = new CreateCategoryRequest("WithBudget", "Has budgets");
        HttpResponseMessage catResponse = await _client.PostAsJsonAsync("/api/v1/categories", categoryRequest);
        CategoryResponse? category = await ReadDataAsync<CategoryResponse>(catResponse);

        var budgetRequest = new CreateBudgetRequest(category!.Id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 1000m);
        await _client.PostAsJsonAsync("/api/v1/budgets", budgetRequest);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/categories/{category.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
