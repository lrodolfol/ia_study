//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace E2ETests;

using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Budgets;
using Application.DTOs.ExpenseItems;
using Domain.Enums;

public class BudgetFlowTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task CreateBudget_StartAndEndDateStoredCorrectly()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "BudgetCat", TransactionType.Expense);
        
        var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2024, 1, 31, 0, 0, 0, DateTimeKind.Utc);
        var request = new CreateBudgetRequest(category.Id, 500m, startDate, endDate);

        var response = await client.PostAsJsonAsync("/api/v1/budgets", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<BudgetResponse>>(TestHelpers.JsonOptions);
        Assert.NotNull(wrapper);
        var result = wrapper.Data!;
        Assert.Equal(500m, result.LimitAmount);
        Assert.Equal("BudgetCat", result.CategoryName);
    }

    [Fact]
    public async Task CreateExpenseWithMultipleItems_AllItemsCreatedSuccessfully()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory);
        var account = await TestDataSeeder.SeedAccountAsync(factory);
        var expense = await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id, 100m);

        var item1 = new CreateExpenseItemRequest(expense.Id, 30m, "Item 1", null);
        var item2 = new CreateExpenseItemRequest(expense.Id, 25m, "Item 2", null);
        var item3 = new CreateExpenseItemRequest(expense.Id, 20m, "Item 3", null);

        var response1 = await client.PostAsJsonAsync("/api/v1/expense-items", item1);
        var response2 = await client.PostAsJsonAsync("/api/v1/expense-items", item2);
        var response3 = await client.PostAsJsonAsync("/api/v1/expense-items", item3);

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response3.StatusCode);
    }

    [Fact]
    public async Task CreateExpenseItem_SumExceedsParent_ReturnsBadRequest()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory);
        var account = await TestDataSeeder.SeedAccountAsync(factory);
        var expense = await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id, 100m);
        
        await TestDataSeeder.SeedExpenseItemAsync(factory, expense.Id, 80m, "First item");

        var request = new CreateExpenseItemRequest(expense.Id, 50m, "Exceeds limit", null);
        var response = await client.PostAsJsonAsync("/api/v1/expense-items", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBudget_ChangesArePersisted()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory);
        var budget = await TestDataSeeder.SeedBudgetAsync(factory, category.Id, 500m);

        var updateRequest = new UpdateBudgetRequest(null, 750m, null, null);
        var response = await client.PutAsJsonAsync($"/api/v1/budgets/{budget.Id}", updateRequest);
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<BudgetResponse>>(TestHelpers.JsonOptions);
        var updated = wrapper!.Data!;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(750m, updated.LimitAmount);
    }
}
