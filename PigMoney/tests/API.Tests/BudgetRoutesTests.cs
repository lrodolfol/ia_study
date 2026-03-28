//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Tests;

using System.Net;
using System.Net.Http.Json;
using API.Routes;
using Application.DTOs.Budgets;
using Application.DTOs.Common;
using Application.Services;
using Domain.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

public class BudgetRoutesTests
{
    private readonly Mock<IBudgetService> _mockService;
    private static readonly DateTime TestDate = DateTime.UtcNow;
    private static readonly DateTime StartDate = new(2024, 1, 1);
    private static readonly DateTime EndDate = new(2024, 1, 31);

    public BudgetRoutesTests()
    {
        _mockService = new Mock<IBudgetService>();
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
                    app.UseEndpoints(endpoints => endpoints.MapBudgetRoutes());
                });
            })
            .Build();
    }

    [Fact]
    public async Task PostBudget_ReturnsCreated_WhenSuccessful()
    {
        var request = new CreateBudgetRequest(1, 500m, StartDate, EndDate);
        var response = new BudgetResponse(1, 1, "Food", 500m, StartDate, EndDate, TestDate, TestDate);
        _mockService.Setup(s => s.CreateBudgetAsync(It.IsAny<CreateBudgetRequest>()))
            .ReturnsAsync(Result<BudgetResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PostAsJsonAsync("/api/v1/budgets", request);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Contains("/api/v1/budgets/1", httpResponse.Headers.Location?.ToString() ?? "");
    }

    [Fact]
    public async Task GetBudgetById_ReturnsOk_WhenFound()
    {
        var response = new BudgetResponse(1, 1, "Food", 500m, StartDate, EndDate, TestDate, TestDate);
        _mockService.Setup(s => s.GetBudgetByIdAsync(1))
            .ReturnsAsync(Result<BudgetResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/budgets/1");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        var result = await httpResponse.Content.ReadFromJsonAsync<BudgetResponse>();
        Assert.Equal("Food", result?.CategoryName);
    }

    [Fact]
    public async Task GetBudgetById_ReturnsNotFound_WhenNotExists()
    {
        _mockService.Setup(s => s.GetBudgetByIdAsync(999))
            .ReturnsAsync(Result<BudgetResponse>.Failure("Budget not found"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/budgets/999");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetBudgets_ReturnsOk_WithPaginatedList()
    {
        var budgets = new List<BudgetResponse>
        {
            new(1, 1, "Food", 500m, StartDate, EndDate, TestDate, TestDate),
            new(2, 2, "Transport", 200m, StartDate, EndDate, TestDate, TestDate)
        };
        var paginatedList = new PaginatedList<BudgetResponse>(budgets, 2, 1, 50);
        _mockService.Setup(s => s.GetAllBudgetsAsync(1, 50))
            .ReturnsAsync(Result<PaginatedList<BudgetResponse>>.Success(paginatedList));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/budgets?page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetBudgetsByCategory_ReturnsOk_WithFilteredResults()
    {
        var budgets = new List<BudgetResponse>
        {
            new(1, 1, "Food", 500m, StartDate, EndDate, TestDate, TestDate),
            new(3, 1, "Food", 600m, new DateTime(2024, 2, 1), new DateTime(2024, 2, 28), TestDate, TestDate)
        };
        var paginatedList = new PaginatedList<BudgetResponse>(budgets, 2, 1, 50);
        _mockService.Setup(s => s.GetBudgetsByCategoryAsync(1, 1, 50))
            .ReturnsAsync(Result<PaginatedList<BudgetResponse>>.Success(paginatedList));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/budgets/by-category/1?page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task PutBudget_ReturnsOk_WhenSuccessful()
    {
        var request = new UpdateBudgetRequest(1, 750m, StartDate, EndDate);
        var response = new BudgetResponse(1, 1, "Food", 750m, StartDate, EndDate, TestDate, TestDate);
        _mockService.Setup(s => s.UpdateBudgetAsync(1, It.IsAny<UpdateBudgetRequest>()))
            .ReturnsAsync(Result<BudgetResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PutAsJsonAsync("/api/v1/budgets/1", request);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteBudget_ReturnsOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.DeleteBudgetAsync(1))
            .ReturnsAsync(Result.Success());

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.DeleteAsync("/api/v1/budgets/1");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }
}
