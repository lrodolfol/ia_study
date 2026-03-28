//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace E2ETests;

using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Expenses;
using Application.DTOs.Incomes;
using Application.DTOs.Categories;
using Application.DTOs.Accounts;
using Application.DTOs.Budgets;
using Domain.Enums;

public class UserFlowTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task UserFlow_RecordingSimpleExpense_CompleteScenario()
    {
        using var client = factory.CreateClient();

        var categoryRequest = new CreateCategoryRequest("Groceries", TransactionType.Expense);
        var categoryResponse = await client.PostAsJsonAsync("/api/v1/categories", categoryRequest);
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        var categoryWrapper = await categoryResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryResponse>>(TestHelpers.JsonOptions);
        var category = categoryWrapper!.Data!;

        var accountRequest = new CreateAccountRequest("Checking Account", AccountType.Checking, 2000m);
        var accountResponse = await client.PostAsJsonAsync("/api/v1/accounts", accountRequest);
        Assert.Equal(HttpStatusCode.Created, accountResponse.StatusCode);
        var accountWrapper = await accountResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>(TestHelpers.JsonOptions);
        var account = accountWrapper!.Data!;

        var expenseRequest = new CreateExpenseRequest(
            Amount: 150.75m,
            Date: DateTime.UtcNow,
            CategoryId: category.Id,
            AccountId: account.Id,
            Description: "Weekly grocery shopping",
            Notes: "At local supermarket"
        );
        var expenseResponse = await client.PostAsJsonAsync("/api/v1/expenses", expenseRequest);
        Assert.Equal(HttpStatusCode.Created, expenseResponse.StatusCode);

        var expenseWrapper = await expenseResponse.Content.ReadFromJsonAsync<ApiResponse<ExpenseResponse>>(TestHelpers.JsonOptions);
        var expense = expenseWrapper!.Data!;

        Assert.Equal(150.75m, expense.Amount);
        Assert.Equal("Groceries", expense.CategoryName);
        Assert.Equal("Checking Account", expense.AccountName);
        Assert.Equal("Weekly grocery shopping", expense.Description);
    }

    [Fact]
    public async Task UserFlow_CreatingMonthlyBudget_CompleteScenario()
    {
        using var client = factory.CreateClient();

        var categoryRequest = new CreateCategoryRequest("Entertainment", TransactionType.Expense);
        var categoryResponse = await client.PostAsJsonAsync("/api/v1/categories", categoryRequest);
        var categoryWrapper = await categoryResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryResponse>>(TestHelpers.JsonOptions);
        var category = categoryWrapper!.Data!;

        var startDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2024, 3, 31, 23, 59, 59, DateTimeKind.Utc);
        var budgetRequest = new CreateBudgetRequest(category.Id, 300m, startDate, endDate);
        var budgetResponse = await client.PostAsJsonAsync("/api/v1/budgets", budgetRequest);
        Assert.Equal(HttpStatusCode.Created, budgetResponse.StatusCode);

        var budgetWrapper = await budgetResponse.Content.ReadFromJsonAsync<ApiResponse<BudgetResponse>>(TestHelpers.JsonOptions);
        var budget = budgetWrapper!.Data!;

        Assert.Equal(300m, budget.LimitAmount);
        Assert.Equal("Entertainment", budget.CategoryName);
        Assert.Equal(category.Id, budget.CategoryId);
    }

    [Fact(Skip = "RecurringTransactions feature not implemented in current API version")]
    public async Task UserFlow_ManagingRecurringBills_NotImplemented()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task UserFlow_TrackingIncome_CompleteScenario()
    {
        using var client = factory.CreateClient();

        var categoryRequest = new CreateCategoryRequest("Salary", TransactionType.Income);
        var categoryResponse = await client.PostAsJsonAsync("/api/v1/categories", categoryRequest);
        var categoryWrapper = await categoryResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryResponse>>(TestHelpers.JsonOptions);
        var category = categoryWrapper!.Data!;

        var accountRequest = new CreateAccountRequest("Bank Account", AccountType.Checking, 0m);
        var accountResponse = await client.PostAsJsonAsync("/api/v1/accounts", accountRequest);
        var accountWrapper = await accountResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>(TestHelpers.JsonOptions);
        var account = accountWrapper!.Data!;

        var incomeRequest = new CreateIncomeRequest(
            Amount: 5000m,
            Date: DateTime.UtcNow,
            CategoryId: category.Id,
            AccountId: account.Id,
            Description: "Monthly salary",
            Notes: "March payment"
        );
        var incomeResponse = await client.PostAsJsonAsync("/api/v1/incomes", incomeRequest);
        Assert.Equal(HttpStatusCode.Created, incomeResponse.StatusCode);

        var incomeWrapper = await incomeResponse.Content.ReadFromJsonAsync<ApiResponse<IncomeResponse>>(TestHelpers.JsonOptions);
        var income = incomeWrapper!.Data!;

        Assert.Equal(5000m, income.Amount);
        Assert.Equal("Salary", income.CategoryName);
    }

    [Fact]
    public async Task UserFlow_ManageMultipleAccounts_CompleteScenario()
    {
        using var client = factory.CreateClient();

        var accounts = new[]
        {
            new CreateAccountRequest("Checking", AccountType.Checking, 5000m),
            new CreateAccountRequest("Savings", AccountType.Savings, 15000m),
            new CreateAccountRequest("Credit Card", AccountType.CreditCard, 0m),
            new CreateAccountRequest("Cash", AccountType.Cash, 200m)
        };

        var createdAccounts = new List<AccountResponse>();
        foreach (var accountRequest in accounts)
        {
            var response = await client.PostAsJsonAsync("/api/v1/accounts", accountRequest);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>(TestHelpers.JsonOptions);
            createdAccounts.Add(wrapper!.Data!);
        }

        Assert.Equal(4, createdAccounts.Count);
        Assert.Contains(createdAccounts, a => a.Name == "Checking");
        Assert.Contains(createdAccounts, a => a.Name == "Savings");
        Assert.Contains(createdAccounts, a => a.Name == "Credit Card");
        Assert.Contains(createdAccounts, a => a.Name == "Cash");
    }

    [Fact]
    public async Task UserFlow_CategorizeTransactions_AcrossCategories()
    {
        using var client = factory.CreateClient();

        var categories = new[]
        {
            new CreateCategoryRequest("Food", TransactionType.Expense),
            new CreateCategoryRequest("Transport", TransactionType.Expense),
            new CreateCategoryRequest("Healthcare", TransactionType.Expense)
        };

        var createdCategories = new List<CategoryResponse>();
        foreach (var categoryRequest in categories)
        {
            var response = await client.PostAsJsonAsync("/api/v1/categories", categoryRequest);
            var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryResponse>>(TestHelpers.JsonOptions);
            createdCategories.Add(wrapper!.Data!);
        }

        var account = await TestDataSeeder.SeedAccountAsync(factory, "MainAccount", AccountType.Checking, 5000m);

        foreach (var category in createdCategories)
        {
            var expenseRequest = new CreateExpenseRequest(50m, DateTime.UtcNow, category.Id, account.Id, $"{category.Name} expense", null);
            var response = await client.PostAsJsonAsync("/api/v1/expenses", expenseRequest);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        var listResponse = await client.GetAsync("/api/v1/expenses?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
    }

    [Fact]
    public async Task UserFlow_UpdateAndCorrectTransaction()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "Utilities", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "BillsAccount", AccountType.Checking, 1000m);

        var createRequest = new CreateExpenseRequest(85m, DateTime.UtcNow, category.Id, account.Id, "Electric bill", null);
        var createResponse = await client.PostAsJsonAsync("/api/v1/expenses", createRequest);
        var createWrapper = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ExpenseResponse>>(TestHelpers.JsonOptions);
        var originalExpense = createWrapper!.Data!;

        var updateRequest = new UpdateExpenseRequest(95.50m, null, null, null, "Electric bill - corrected", "January bill");
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/expenses/{originalExpense.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/expenses/{originalExpense.Id}");
        var getWrapper = await getResponse.Content.ReadFromJsonAsync<ApiResponse<ExpenseResponse>>(TestHelpers.JsonOptions);
        var updatedExpense = getWrapper!.Data!;

        Assert.Equal(95.50m, updatedExpense.Amount);
        Assert.Equal("Electric bill - corrected", updatedExpense.Description);
    }

    [Fact]
    public async Task UserFlow_DeleteErroneousEntry()
    {
        using var client = factory.CreateClient();
        var category = await TestDataSeeder.SeedCategoryAsync(factory, "Misc", TransactionType.Expense);
        var account = await TestDataSeeder.SeedAccountAsync(factory, "MiscAcc", AccountType.Cash, 500m);

        var createRequest = new CreateExpenseRequest(999m, DateTime.UtcNow, category.Id, account.Id, "Wrong entry", null);
        var createResponse = await client.PostAsJsonAsync("/api/v1/expenses", createRequest);
        var wrapper = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ExpenseResponse>>(TestHelpers.JsonOptions);
        var expense = wrapper!.Data!;

        var deleteResponse = await client.DeleteAsync($"/api/v1/expenses/{expense.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/expenses/{expense.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
