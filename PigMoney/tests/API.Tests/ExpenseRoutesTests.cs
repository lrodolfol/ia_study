//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Tests;

using System.Net;
using System.Net.Http.Json;
using API.Routes;
using Application.DTOs.Expenses;
using Application.DTOs.Common;
using Application.Services;
using Domain.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

public class ExpenseRoutesTests
{
    private readonly Mock<IExpenseService> _mockService;
    private static readonly DateTime TestDate = DateTime.UtcNow;

    public ExpenseRoutesTests()
    {
        _mockService = new Mock<IExpenseService>();
    }

    private IHost CreateTestHost()
    {
        return new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddSingleton(_mockService.Object);
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapExpenseRoutes());
                });
            })
            .Build();
    }

    [Fact]
    public async Task PostExpense_ReturnsCreated_WhenSuccessful()
    {
        var request = new CreateExpenseRequest(25.50m, TestDate, 1, 1, "Lunch", "Notes");
        var response = new ExpenseResponse(1, 25.50m, TestDate, 1, "Food", 1, "Bank", "Lunch", "Notes", TestDate, TestDate);
        _mockService.Setup(s => s.CreateExpenseAsync(It.IsAny<CreateExpenseRequest>()))
            .ReturnsAsync(Result<ExpenseResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PostAsJsonAsync("/api/v1/expenses", request);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Contains("/api/v1/expenses/1", httpResponse.Headers.Location?.ToString() ?? "");
    }

    [Fact]
    public async Task PostExpense_ReturnsBadRequest_WhenValidationFails()
    {
        var request = new CreateExpenseRequest(-100m, TestDate, 1, 1, "", null);
        _mockService.Setup(s => s.CreateExpenseAsync(It.IsAny<CreateExpenseRequest>()))
            .ReturnsAsync(Result<ExpenseResponse>.Failure("Amount must be positive"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PostAsJsonAsync("/api/v1/expenses", request);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetExpenseById_ReturnsOk_WhenFound()
    {
        var response = new ExpenseResponse(1, 25.50m, TestDate, 1, "Food", 1, "Bank", "Lunch", "Notes", TestDate, TestDate);
        _mockService.Setup(s => s.GetExpenseByIdAsync(1))
            .ReturnsAsync(Result<ExpenseResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/expenses/1");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        var result = await httpResponse.Content.ReadFromJsonAsync<ExpenseResponse>();
        Assert.Equal("Lunch", result?.Description);
    }

    [Fact]
    public async Task GetExpenseById_ReturnsNotFound_WhenNotExists()
    {
        _mockService.Setup(s => s.GetExpenseByIdAsync(999))
            .ReturnsAsync(Result<ExpenseResponse>.Failure("Expense not found"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/expenses/999");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetExpenses_ReturnsOk_WithFilters()
    {
        var expenses = new List<ExpenseResponse>
        {
            new(1, 25.50m, TestDate, 1, "Food", 1, "Bank", "Lunch", "Notes", TestDate, TestDate)
        };
        var paginatedList = new PaginatedList<ExpenseResponse>(expenses, 1, 1, 50);
        _mockService.Setup(s => s.GetExpensesAsync(It.Is<ExpenseFilterParams>(f => 
            f.CategoryId == 1 && f.Page == 1 && f.PageSize == 50)))
            .ReturnsAsync(Result<PaginatedList<ExpenseResponse>>.Success(paginatedList));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/expenses?categoryId=1&page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetExpenses_FiltersByDateRange()
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var expenses = new List<ExpenseResponse>
        {
            new(1, 25.50m, TestDate.AddDays(-3), 1, "Food", 1, "Bank", "Lunch", "Notes", TestDate, TestDate)
        };
        var paginatedList = new PaginatedList<ExpenseResponse>(expenses, 1, 1, 50);
        _mockService.Setup(s => s.GetExpensesAsync(It.Is<ExpenseFilterParams>(f => 
            f.StartDate != null && f.EndDate != null)))
            .ReturnsAsync(Result<PaginatedList<ExpenseResponse>>.Success(paginatedList));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync($"/api/v1/expenses?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetExpenses_PaginationWorksCorrectly()
    {
        var expenses = new List<ExpenseResponse>();
        for (int i = 1; i <= 10; i++)
        {
            expenses.Add(new(i, 10m * i, TestDate, 1, "Food", 1, "Bank", $"Expense {i}", "Notes", TestDate, TestDate));
        }
        var paginatedList = new PaginatedList<ExpenseResponse>(expenses, 100, 2, 10);
        _mockService.Setup(s => s.GetExpensesAsync(It.Is<ExpenseFilterParams>(f => 
            f.Page == 2 && f.PageSize == 10)))
            .ReturnsAsync(Result<PaginatedList<ExpenseResponse>>.Success(paginatedList));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/expenses?page=2&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task PutExpense_ReturnsOk_WhenSuccessful()
    {
        var request = new UpdateExpenseRequest(30m, TestDate, 1, 1, "Updated Lunch", "Updated");
        var response = new ExpenseResponse(1, 30m, TestDate, 1, "Food", 1, "Bank", "Updated Lunch", "Updated", TestDate, TestDate);
        _mockService.Setup(s => s.UpdateExpenseAsync(1, It.IsAny<UpdateExpenseRequest>()))
            .ReturnsAsync(Result<ExpenseResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PutAsJsonAsync("/api/v1/expenses/1", request);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task PutExpense_ReturnsNotFound_WhenNotExists()
    {
        var request = new UpdateExpenseRequest(30m, TestDate, 1, 1, "Updated", null);
        _mockService.Setup(s => s.UpdateExpenseAsync(999, It.IsAny<UpdateExpenseRequest>()))
            .ReturnsAsync(Result<ExpenseResponse>.Failure("Expense not found"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PutAsJsonAsync("/api/v1/expenses/999", request);

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteExpense_ReturnsOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.DeleteExpenseAsync(1))
            .ReturnsAsync(Result.Success());

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.DeleteAsync("/api/v1/expenses/1");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }
}
