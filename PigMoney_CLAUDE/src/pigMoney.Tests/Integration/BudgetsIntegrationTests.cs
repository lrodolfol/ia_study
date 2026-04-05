//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Budgets;
using Application.DTOs.Categories;
using Application.DTOs.Common;

namespace pigMoney.Tests.Integration;

public class BudgetsIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public BudgetsIntegrationTests(TestWebApplicationFactory factory)
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

    private async Task<CategoryResponse> CreateTestCategoryAsync()
    {
        var request = new CreateCategoryRequest("Budget Category", "Test category for budgets");
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/categories", request);
        response.EnsureSuccessStatusCode();
        return (await ReadDataAsync<CategoryResponse>(response))!;
    }

    [Fact]
    public async Task PostBudget_ShouldReturn201WithLocationHeader()
    {
        CategoryResponse category = await CreateTestCategoryAsync();
        var request = new CreateBudgetRequest(category.Id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 5000m);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/budgets", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        await AssertEnvelopeAsync(response, 201);

        BudgetResponse? created = await ReadDataAsync<BudgetResponse>(response);
        Assert.NotNull(created);
        Assert.Equal(5000m, created.LimitAmount);
        Assert.Equal(category.Id, created.CategoryId);
    }

    [Fact]
    public async Task PostBudget_WithInvalidDates_ShouldReturn400()
    {
        CategoryResponse category = await CreateTestCategoryAsync();
        var startDate = DateTime.UtcNow.AddMonths(1);
        var endDate = DateTime.UtcNow;
        var request = new CreateBudgetRequest(category.Id, startDate, endDate, 1000m);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/budgets", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertEnvelopeAsync(response, 400);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement errorArray = doc.RootElement.GetProperty("error");
        Assert.True(errorArray.GetArrayLength() > 0);
    }

    [Fact]
    public async Task PostBudget_WithZeroLimitAmount_ShouldReturn400()
    {
        CategoryResponse category = await CreateTestCategoryAsync();
        var request = new CreateBudgetRequest(category.Id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 0m);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/budgets", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBudgets_WithFilters_ShouldReturn200WithPagination()
    {
        CategoryResponse category = await CreateTestCategoryAsync();
        var request = new CreateBudgetRequest(category.Id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 3000m);
        await _client.PostAsJsonAsync("/api/v1/budgets", request);

        HttpResponseMessage response = await _client.GetAsync(
            $"/api/v1/budgets?page=1&pageSize=10&categoryId={category.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        PaginatedList<BudgetResponse>? result = await ReadDataAsync<PaginatedList<BudgetResponse>>(response);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetBudgetById_ShouldReturn200()
    {
        CategoryResponse category = await CreateTestCategoryAsync();
        var request = new CreateBudgetRequest(category.Id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 2000m);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/budgets", request);
        BudgetResponse? created = await ReadDataAsync<BudgetResponse>(createResponse);

        HttpResponseMessage response = await _client.GetAsync($"/api/v1/budgets/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        BudgetResponse? budget = await ReadDataAsync<BudgetResponse>(response);
        Assert.NotNull(budget);
        Assert.Equal(created.Id, budget.Id);
    }

    [Fact]
    public async Task GetBudgetById_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/budgets/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertEnvelopeAsync(response, 404);
    }

    [Fact]
    public async Task PutBudget_ShouldReturn200()
    {
        CategoryResponse category = await CreateTestCategoryAsync();
        var createRequest = new CreateBudgetRequest(category.Id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 1000m);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/budgets", createRequest);
        BudgetResponse? created = await ReadDataAsync<BudgetResponse>(createResponse);

        var updateRequest = new UpdateBudgetRequest(category.Id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(2), 8000m);
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/budgets/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertEnvelopeAsync(response, 200);

        BudgetResponse? updated = await ReadDataAsync<BudgetResponse>(response);
        Assert.Equal(8000m, updated!.LimitAmount);
    }

    [Fact]
    public async Task PutBudget_WhenNotFound_ShouldReturn404()
    {
        CategoryResponse category = await CreateTestCategoryAsync();
        var request = new UpdateBudgetRequest(category.Id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 100m);
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/budgets/9999", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBudget_ShouldReturn204()
    {
        CategoryResponse category = await CreateTestCategoryAsync();
        var createRequest = new CreateBudgetRequest(category.Id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 500m);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/budgets", createRequest);
        BudgetResponse? created = await ReadDataAsync<BudgetResponse>(createResponse);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/budgets/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBudget_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.DeleteAsync("/api/v1/budgets/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
