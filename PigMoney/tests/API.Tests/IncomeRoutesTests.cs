//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Tests;

using System.Net;
using System.Net.Http.Json;
using API.Routes;
using Application.DTOs.Incomes;
using Application.DTOs.Common;
using Application.Services;
using Domain.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

public class IncomeRoutesTests
{
    private readonly Mock<IIncomeService> _mockService;
    private static readonly DateTime TestDate = DateTime.UtcNow;

    public IncomeRoutesTests()
    {
        _mockService = new Mock<IIncomeService>();
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
                    app.UseEndpoints(endpoints => endpoints.MapIncomeRoutes());
                });
            })
            .Build();
    }

    [Fact]
    public async Task PostIncome_ReturnsCreated_WhenSuccessful()
    {
        var request = new CreateIncomeRequest(5000m, TestDate, 1, 1, "Salary", "Monthly");
        var response = new IncomeResponse(1, 5000m, TestDate, 1, "Income", 1, "Bank", "Salary", "Monthly", TestDate, TestDate);
        _mockService.Setup(s => s.CreateIncomeAsync(It.IsAny<CreateIncomeRequest>()))
            .ReturnsAsync(Result<IncomeResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PostAsJsonAsync("/api/v1/incomes", request);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Contains("/api/v1/incomes/1", httpResponse.Headers.Location?.ToString() ?? "");
    }

    [Fact]
    public async Task GetIncomeById_ReturnsOk_WhenFound()
    {
        var response = new IncomeResponse(1, 5000m, TestDate, 1, "Income", 1, "Bank", "Salary", "Monthly", TestDate, TestDate);
        _mockService.Setup(s => s.GetIncomeByIdAsync(1))
            .ReturnsAsync(Result<IncomeResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/incomes/1");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        var result = await httpResponse.Content.ReadFromJsonAsync<IncomeResponse>();
        Assert.Equal("Salary", result?.Description);
    }

    [Fact]
    public async Task GetIncomeById_ReturnsNotFound_WhenNotExists()
    {
        _mockService.Setup(s => s.GetIncomeByIdAsync(999))
            .ReturnsAsync(Result<IncomeResponse>.Failure("Income not found"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/incomes/999");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetIncomes_ReturnsOk_WithFilters()
    {
        var incomes = new List<IncomeResponse>
        {
            new(1, 5000m, TestDate, 1, "Income", 1, "Bank", "Salary", "Notes", TestDate, TestDate)
        };
        var paginatedList = new PaginatedList<IncomeResponse>(incomes, 1, 1, 50);
        _mockService.Setup(s => s.GetIncomesAsync(It.IsAny<IncomeFilterParams>()))
            .ReturnsAsync(Result<PaginatedList<IncomeResponse>>.Success(paginatedList));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/incomes?accountId=1&page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task PutIncome_ReturnsOk_WhenSuccessful()
    {
        var request = new UpdateIncomeRequest(5500m, TestDate, 1, 1, "Updated Salary", "Raise");
        var response = new IncomeResponse(1, 5500m, TestDate, 1, "Income", 1, "Bank", "Updated Salary", "Raise", TestDate, TestDate);
        _mockService.Setup(s => s.UpdateIncomeAsync(1, It.IsAny<UpdateIncomeRequest>()))
            .ReturnsAsync(Result<IncomeResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PutAsJsonAsync("/api/v1/incomes/1", request);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteIncome_ReturnsOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.DeleteIncomeAsync(1))
            .ReturnsAsync(Result.Success());

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.DeleteAsync("/api/v1/incomes/1");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }
}
