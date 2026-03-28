//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace E2ETests;

using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Categories;
using Application.DTOs.Accounts;
using Domain.Enums;

public class DataIntegrityTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task DeleteCategory_WithActiveExpenses_ReturnsBadRequest()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "ToDeleteCat", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory);
        await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id, 100m);

        var response = await client.DeleteAsync($"/api/v1/categories/{category.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WithoutDependencies_ReturnsOk()
    {
        using var client = factory.CreateClient();
        var categoryRequest = new CreateCategoryRequest("EmptyCat", TransactionType.Expense);
        var createResponse = await client.PostAsJsonAsync("/api/v1/categories", categoryRequest);
        var wrapper = await createResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryResponse>>(TestHelpers.JsonOptions);
        var category = wrapper!.Data!;

        var deleteResponse = await client.DeleteAsync($"/api/v1/categories/{category.Id}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        
        var getResponse = await client.GetAsync($"/api/v1/categories/{category.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_WithActiveTransactions_ReturnsBadRequest()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "ToDeleteAcc", AccountType.Checking, 1000m);
        await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id, 50m);

        var response = await client.DeleteAsync($"/api/v1/accounts/{account.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_WithoutDependencies_ReturnsOk()
    {
        using var client = factory.CreateClient();
        var accountRequest = new CreateAccountRequest("EmptyAcc", AccountType.Cash, 0m);
        var createResponse = await client.PostAsJsonAsync("/api/v1/accounts", accountRequest);
        var wrapper = await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>(TestHelpers.JsonOptions);
        var account = wrapper!.Data!;

        var deleteResponse = await client.DeleteAsync($"/api/v1/accounts/{account.Id}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        
        var getResponse = await client.GetAsync($"/api/v1/accounts/{account.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task ExpenseItemsSumValidation_ApplicationLevel_Works()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory);
        var account = await TestDataSeeder.SeedAccountAsync(factory);
        var expense = await TestDataSeeder.SeedExpenseAsync(factory, category.Id, account.Id, 100m);

        await TestDataSeeder.SeedExpenseItemAsync(factory, expense.Id, 40m, "Item1");
        await TestDataSeeder.SeedExpenseItemAsync(factory, expense.Id, 40m, "Item2");

        var request = new Application.DTOs.ExpenseItems.CreateExpenseItemRequest(expense.Id, 30m, "Exceeds", null);
        var response = await client.PostAsJsonAsync("/api/v1/expense-items", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
