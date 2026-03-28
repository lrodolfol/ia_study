//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace E2ETests;

using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Expenses;
using Application.DTOs.Categories;
using Domain.Enums;

public class ExpenseFlowTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task CreateExpense_ReturnsExpenseWithAllFields()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "Groceries", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "Checking", AccountType.Checking, 1000m);
        
        var request = new CreateExpenseRequest(
            Amount: 50.25m,
            Date: DateTime.UtcNow,
            CategoryId: category.Id,
            AccountId: account.Id,
            Description: "Weekly groceries",
            Notes: "At supermarket"
        );

        var response = await client.PostAsJsonAsync("/api/v1/expenses", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<ExpenseResponse>>(TestHelpers.JsonOptions);
        Assert.NotNull(wrapper);
        var result = wrapper.Data;
        Assert.NotNull(result);
        Assert.Equal(50.25m, result.Amount);
        Assert.Equal("Weekly groceries", result.Description);
        Assert.Equal("Groceries", result.CategoryName);
        Assert.Equal("Checking", result.AccountName);
    }

    [Fact]
    public async Task CreateExpense_ThenRetrieve_FieldsMatch()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "Food", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "Bank", AccountType.Checking, 500m);
        
        var request = new CreateExpenseRequest(25.50m, DateTime.UtcNow, category.Id, account.Id, "Lunch", "Work lunch");
        var createResponse = await client.PostAsJsonAsync("/api/v1/expenses", request);
        var createWrapper = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ExpenseResponse>>(TestHelpers.JsonOptions);
        var created = createWrapper!.Data!;

        var getResponse = await client.GetAsync($"/api/v1/expenses/{created.Id}");
        var getWrapper = await getResponse.Content.ReadFromJsonAsync<ApiResponse<ExpenseResponse>>(TestHelpers.JsonOptions);
        var retrieved = getWrapper!.Data;

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(created.Amount, retrieved.Amount);
        Assert.Equal(created.Description, retrieved.Description);
    }

    [Fact]
    public async Task CreateCategory_CreateExpense_VerifyCategoryNameInResponse()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "Entertainment2", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "Savings2", AccountType.Savings, 2000m);
        
        var expenseRequest = new CreateExpenseRequest(150m, DateTime.UtcNow, category.Id, account.Id, "Concert tickets", null);
        var expenseResponse = await client.PostAsJsonAsync("/api/v1/expenses", expenseRequest);
        
        Assert.Equal(HttpStatusCode.Created, expenseResponse.StatusCode);
        var expenseWrapper = await expenseResponse.Content.ReadFromJsonAsync<ApiResponse<ExpenseResponse>>(TestHelpers.JsonOptions);
        Assert.NotNull(expenseWrapper);
        Assert.NotNull(expenseWrapper.Data);
        Assert.Equal("Entertainment2", expenseWrapper.Data.CategoryName);
    }

    [Fact]
    public async Task GetExpense_NotFound_Returns404()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/expenses/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateExpense_ChangesArePersisted()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory);
        var account = await TestDataSeeder.SeedAccountAsync(factory);
        var expense = await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id, 100m, "Original");

        var updateRequest = new UpdateExpenseRequest(200m, null, null, null, "Updated", "New notes");
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/expenses/{expense.Id}", updateRequest);
        var wrapper = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<ExpenseResponse>>(TestHelpers.JsonOptions);
        var updated = wrapper!.Data!;

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(200m, updated.Amount);
        Assert.Equal("Updated", updated.Description);
    }

    [Fact]
    public async Task DeleteExpense_SoftDeletes_NoLongerReturned()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory);
        var account = await TestDataSeeder.SeedAccountAsync(factory);
        var expense = await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id);

        var deleteResponse = await client.DeleteAsync($"/api/v1/expenses/{expense.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/expenses/{expense.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
