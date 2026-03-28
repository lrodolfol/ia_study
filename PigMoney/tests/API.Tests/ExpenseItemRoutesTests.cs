//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Tests;

using System.Net;
using System.Net.Http.Json;
using API.Routes;
using Application.DTOs.ExpenseItems;
using Application.DTOs.Common;
using Application.Services;
using Domain.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

public class ExpenseItemRoutesTests
{
    private readonly Mock<IExpenseItemService> _mockService;
    private static readonly DateTime TestDate = DateTime.UtcNow;

    public ExpenseItemRoutesTests()
    {
        _mockService = new Mock<IExpenseItemService>();
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
                    app.UseEndpoints(endpoints => endpoints.MapExpenseItemRoutes());
                });
            })
            .Build();
    }

    [Fact]
    public async Task PostExpenseItem_ReturnsCreated_WhenSuccessful()
    {
        var request = new CreateExpenseItemRequest(1, 25m, "Item 1", null);
        var response = new ExpenseItemResponse(1, 1, 25m, "Item 1", null, null, TestDate, TestDate);
        _mockService.Setup(s => s.CreateExpenseItemAsync(It.IsAny<CreateExpenseItemRequest>()))
            .ReturnsAsync(Result<ExpenseItemResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PostAsJsonAsync("/api/v1/expense-items", request);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Contains("/api/v1/expense-items/1", httpResponse.Headers.Location?.ToString() ?? "");
    }

    [Fact]
    public async Task PostExpenseItem_ReturnsBadRequest_WhenSumExceedsExpense()
    {
        var request = new CreateExpenseItemRequest(1, 1000m, "Big Item", null);
        _mockService.Setup(s => s.CreateExpenseItemAsync(It.IsAny<CreateExpenseItemRequest>()))
            .ReturnsAsync(Result<ExpenseItemResponse>.Failure("Sum of expense items cannot exceed expense amount"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PostAsJsonAsync("/api/v1/expense-items", request);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetExpenseItemById_ReturnsOk_WhenFound()
    {
        var response = new ExpenseItemResponse(1, 1, 25m, "Item 1", null, null, TestDate, TestDate);
        _mockService.Setup(s => s.GetExpenseItemByIdAsync(1))
            .ReturnsAsync(Result<ExpenseItemResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/expense-items/1");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        var result = await httpResponse.Content.ReadFromJsonAsync<ExpenseItemResponse>();
        Assert.Equal("Item 1", result?.Description);
    }

    [Fact]
    public async Task GetExpenseItemById_ReturnsNotFound_WhenNotExists()
    {
        _mockService.Setup(s => s.GetExpenseItemByIdAsync(999))
            .ReturnsAsync(Result<ExpenseItemResponse>.Failure("ExpenseItem not found"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/expense-items/999");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetExpenseItemsByExpenseId_ReturnsOk_WithPaginatedList()
    {
        var items = new List<ExpenseItemResponse>
        {
            new(1, 1, 25m, "Item 1", null, null, TestDate, TestDate),
            new(2, 1, 30m, "Item 2", null, null, TestDate, TestDate)
        };
        var paginatedList = new PaginatedList<ExpenseItemResponse>(items, 2, 1, 50);
        _mockService.Setup(s => s.GetExpenseItemsByExpenseIdAsync(1, 1, 50))
            .ReturnsAsync(Result<PaginatedList<ExpenseItemResponse>>.Success(paginatedList));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/expense-items/by-expense/1?page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task PutExpenseItem_ReturnsOk_WhenSuccessful()
    {
        var request = new UpdateExpenseItemRequest(35m, "Updated Item", null);
        var response = new ExpenseItemResponse(1, 1, 35m, "Updated Item", null, null, TestDate, TestDate);
        _mockService.Setup(s => s.UpdateExpenseItemAsync(1, It.IsAny<UpdateExpenseItemRequest>()))
            .ReturnsAsync(Result<ExpenseItemResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PutAsJsonAsync("/api/v1/expense-items/1", request);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task PutExpenseItem_ReturnsNotFound_WhenSumExceedsExpense()
    {
        var request = new UpdateExpenseItemRequest(1000m, "Updated Item", null);
        _mockService.Setup(s => s.UpdateExpenseItemAsync(1, It.IsAny<UpdateExpenseItemRequest>()))
            .ReturnsAsync(Result<ExpenseItemResponse>.Failure("Sum of expense items cannot exceed expense amount"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PutAsJsonAsync("/api/v1/expense-items/1", request);

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteExpenseItem_ReturnsOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.DeleteExpenseItemAsync(1))
            .ReturnsAsync(Result.Success());

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.DeleteAsync("/api/v1/expense-items/1");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }
}
