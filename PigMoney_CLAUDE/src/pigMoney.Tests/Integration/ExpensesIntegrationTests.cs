//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Accounts;
using Application.DTOs.Categories;
using Application.DTOs.Common;
using Application.DTOs.Expenses;
using Application.DTOs.ExpenseItems;
using Domain.Enums;

namespace pigMoney.Tests.Integration;

public class ExpensesIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ExpensesIntegrationTests(TestWebApplicationFactory factory)
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
        var request = new CreateAccountRequest("Expense Account", AccountType.Cash, 10000m);
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/accounts", request);
        response.EnsureSuccessStatusCode();
        return (await ReadDataAsync<AccountResponse>(response))!;
    }

    private async Task<CategoryResponse> CreateTestCategoryAsync()
    {
        var request = new CreateCategoryRequest("Expense Category", "For expenses");
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/categories", request);
        response.EnsureSuccessStatusCode();
        return (await ReadDataAsync<CategoryResponse>(response))!;
    }

    [Fact]
    public async Task PostExpense_ShouldReturn201WithLocationHeader()
    {
        AccountResponse account = await CreateTestAccountAsync();
        CategoryResponse category = await CreateTestCategoryAsync();
        var request = new CreateExpenseRequest(250m, DateTime.UtcNow, "Grocery shopping", account.Id, category.Id);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/expenses", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        await AssertEnvelopeAsync(response, 201);

        ExpenseResponse? created = await ReadDataAsync<ExpenseResponse>(response);
        Assert.NotNull(created);
        Assert.Equal(250m, created.Amount);
        Assert.Equal("Grocery shopping", created.Description);
        Assert.Equal(account.Id, created.AccountId);
        Assert.Equal(category.Id, created.CategoryId);
    }

    [Fact]
    public async Task PostExpense_WithZeroAmount_ShouldReturn400()
    {
        AccountResponse account = await CreateTestAccountAsync();
        CategoryResponse category = await CreateTestCategoryAsync();
        var request = new CreateExpenseRequest(0m, DateTime.UtcNow, "Invalid", account.Id, category.Id);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/expenses", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertEnvelopeAsync(response, 400);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement errorArray = doc.RootElement.GetProperty("error");
        Assert.True(errorArray.GetArrayLength() > 0);
    }

    [Fact]
    public async Task PostExpense_WithEmptyDescription_ShouldReturn400()
    {
        AccountResponse account = await CreateTestAccountAsync();
        CategoryResponse category = await CreateTestCategoryAsync();
        var request = new CreateExpenseRequest(100m, DateTime.UtcNow, "", account.Id, category.Id);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/expenses", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetExpenses_WithFilters_ShouldReturn200WithPagination()
    {
        AccountResponse account = await CreateTestAccountAsync();
        CategoryResponse category = await CreateTestCategoryAsync();
        var request = new CreateExpenseRequest(100m, DateTime.UtcNow, "Filtered", account.Id, category.Id);
        await _client.PostAsJsonAsync("/api/v1/expenses", request);

        HttpResponseMessage response = await _client.GetAsync(
            $"/api/v1/expenses?page=1&pageSize=10&accountId={account.Id}&categoryId={category.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        PaginatedList<ExpenseResponse>? result = await ReadDataAsync<PaginatedList<ExpenseResponse>>(response);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetExpenseById_ShouldReturn200()
    {
        AccountResponse account = await CreateTestAccountAsync();
        CategoryResponse category = await CreateTestCategoryAsync();
        var request = new CreateExpenseRequest(75m, DateTime.UtcNow, "Lunch", account.Id, category.Id);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/expenses", request);
        ExpenseResponse? created = await ReadDataAsync<ExpenseResponse>(createResponse);

        HttpResponseMessage response = await _client.GetAsync($"/api/v1/expenses/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        ExpenseResponse? expense = await ReadDataAsync<ExpenseResponse>(response);
        Assert.NotNull(expense);
        Assert.Equal(created.Id, expense.Id);
    }

    [Fact]
    public async Task GetExpenseById_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/expenses/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertEnvelopeAsync(response, 404);
    }

    [Fact]
    public async Task PutExpense_ShouldReturn200()
    {
        AccountResponse account = await CreateTestAccountAsync();
        CategoryResponse category = await CreateTestCategoryAsync();
        var createRequest = new CreateExpenseRequest(100m, DateTime.UtcNow, "Original", account.Id, category.Id);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/expenses", createRequest);
        ExpenseResponse? created = await ReadDataAsync<ExpenseResponse>(createResponse);

        var updateRequest = new UpdateExpenseRequest(200m, DateTime.UtcNow, "Updated", account.Id, category.Id);
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/expenses/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        ExpenseResponse? updated = await ReadDataAsync<ExpenseResponse>(response);
        Assert.Equal(200m, updated!.Amount);
        Assert.Equal("Updated", updated.Description);
    }

    [Fact]
    public async Task PutExpense_WhenNotFound_ShouldReturn404()
    {
        AccountResponse account = await CreateTestAccountAsync();
        CategoryResponse category = await CreateTestCategoryAsync();
        var request = new UpdateExpenseRequest(100m, DateTime.UtcNow, "Ghost", account.Id, category.Id);
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/expenses/9999", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExpense_WhenNoItems_ShouldReturn204()
    {
        AccountResponse account = await CreateTestAccountAsync();
        CategoryResponse category = await CreateTestCategoryAsync();
        var createRequest = new CreateExpenseRequest(50m, DateTime.UtcNow, "ToDelete", account.Id, category.Id);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/expenses", createRequest);
        ExpenseResponse? created = await ReadDataAsync<ExpenseResponse>(createResponse);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/expenses/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExpense_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.DeleteAsync("/api/v1/expenses/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExpense_WithItems_ShouldReturn409()
    {
        AccountResponse account = await CreateTestAccountAsync();
        CategoryResponse category = await CreateTestCategoryAsync();
        var expenseRequest = new CreateExpenseRequest(300m, DateTime.UtcNow, "WithItems", account.Id, category.Id);
        HttpResponseMessage expResponse = await _client.PostAsJsonAsync("/api/v1/expenses", expenseRequest);
        ExpenseResponse? expense = await ReadDataAsync<ExpenseResponse>(expResponse);

        var itemRequest = new CreateExpenseItemRequest("Item1", 2m, 50m);
        await _client.PostAsJsonAsync($"/api/v1/expenses/{expense!.Id}/items", itemRequest);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/expenses/{expense.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertEnvelopeAsync(response, 409);
    }
}
