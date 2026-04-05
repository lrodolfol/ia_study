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

public class ExpenseItemsIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ExpenseItemsIntegrationTests(TestWebApplicationFactory factory)
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

    private async Task<ExpenseResponse> CreateTestExpenseAsync()
    {
        var accountRequest = new CreateAccountRequest("Item Account", AccountType.Cash, 10000m);
        HttpResponseMessage accResponse = await _client.PostAsJsonAsync("/api/v1/accounts", accountRequest);
        AccountResponse? account = await ReadDataAsync<AccountResponse>(accResponse);

        var categoryRequest = new CreateCategoryRequest("Item Category", "For items");
        HttpResponseMessage catResponse = await _client.PostAsJsonAsync("/api/v1/categories", categoryRequest);
        CategoryResponse? category = await ReadDataAsync<CategoryResponse>(catResponse);

        var expenseRequest = new CreateExpenseRequest(500m, DateTime.UtcNow, "Test Expense", account!.Id, category!.Id);
        HttpResponseMessage expResponse = await _client.PostAsJsonAsync("/api/v1/expenses", expenseRequest);
        return (await ReadDataAsync<ExpenseResponse>(expResponse))!;
    }

    [Fact]
    public async Task PostExpenseItem_ShouldReturn201()
    {
        ExpenseResponse expense = await CreateTestExpenseAsync();
        var request = new CreateExpenseItemRequest("Rice", 2m, 5.50m);

        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/expenses/{expense.Id}/items", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await AssertEnvelopeAsync(response, 201);

        ExpenseItemResponse? created = await ReadDataAsync<ExpenseItemResponse>(response);
        Assert.NotNull(created);
        Assert.Equal("Rice", created.Name);
        Assert.Equal(2m, created.Quantity);
        Assert.Equal(5.50m, created.UnitPrice);
        Assert.Equal(expense.Id, created.ExpenseId);
    }

    [Fact]
    public async Task PostExpenseItem_WithEmptyName_ShouldReturn400()
    {
        ExpenseResponse expense = await CreateTestExpenseAsync();
        var request = new CreateExpenseItemRequest("", 1m, 10m);

        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/expenses/{expense.Id}/items", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertEnvelopeAsync(response, 400);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement errorArray = doc.RootElement.GetProperty("error");
        Assert.True(errorArray.GetArrayLength() > 0);
    }

    [Fact]
    public async Task PostExpenseItem_WithZeroQuantity_ShouldReturn400()
    {
        ExpenseResponse expense = await CreateTestExpenseAsync();
        var request = new CreateExpenseItemRequest("Item", 0m, 10m);

        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/expenses/{expense.Id}/items", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetItemsByExpenseId_ShouldReturn200WithPagination()
    {
        ExpenseResponse expense = await CreateTestExpenseAsync();
        var itemRequest = new CreateExpenseItemRequest("Beans", 3m, 4m);
        await _client.PostAsJsonAsync($"/api/v1/expenses/{expense.Id}/items", itemRequest);

        HttpResponseMessage response = await _client.GetAsync(
            $"/api/v1/expenses/{expense.Id}/items?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        PaginatedList<ExpenseItemResponse>? result = await ReadDataAsync<PaginatedList<ExpenseItemResponse>>(response);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetExpenseItemById_ShouldReturn200()
    {
        ExpenseResponse expense = await CreateTestExpenseAsync();
        var itemRequest = new CreateExpenseItemRequest("Milk", 1m, 6m);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync(
            $"/api/v1/expenses/{expense.Id}/items", itemRequest);
        ExpenseItemResponse? created = await ReadDataAsync<ExpenseItemResponse>(createResponse);

        HttpResponseMessage response = await _client.GetAsync($"/api/v1/expense-items/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        ExpenseItemResponse? item = await ReadDataAsync<ExpenseItemResponse>(response);
        Assert.NotNull(item);
        Assert.Equal(created.Id, item.Id);
        Assert.Equal("Milk", item.Name);
    }

    [Fact]
    public async Task GetExpenseItemById_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/expense-items/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertEnvelopeAsync(response, 404);
    }

    [Fact]
    public async Task PutExpenseItem_ShouldReturn200()
    {
        ExpenseResponse expense = await CreateTestExpenseAsync();
        var createRequest = new CreateExpenseItemRequest("Original", 1m, 10m);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync(
            $"/api/v1/expenses/{expense.Id}/items", createRequest);
        ExpenseItemResponse? created = await ReadDataAsync<ExpenseItemResponse>(createResponse);

        var updateRequest = new UpdateExpenseItemRequest("Updated", 5m, 20m);
        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/v1/expense-items/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        ExpenseItemResponse? updated = await ReadDataAsync<ExpenseItemResponse>(response);
        Assert.Equal("Updated", updated!.Name);
        Assert.Equal(5m, updated.Quantity);
        Assert.Equal(20m, updated.UnitPrice);
    }

    [Fact]
    public async Task PutExpenseItem_WhenNotFound_ShouldReturn404()
    {
        var request = new UpdateExpenseItemRequest("Ghost", 1m, 1m);
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/expense-items/9999", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExpenseItem_ShouldReturn204()
    {
        ExpenseResponse expense = await CreateTestExpenseAsync();
        var createRequest = new CreateExpenseItemRequest("ToDelete", 1m, 5m);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync(
            $"/api/v1/expenses/{expense.Id}/items", createRequest);
        ExpenseItemResponse? created = await ReadDataAsync<ExpenseItemResponse>(createResponse);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/expense-items/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExpenseItem_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.DeleteAsync("/api/v1/expense-items/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
