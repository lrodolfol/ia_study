//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Tests;

using System.Net;
using System.Net.Http.Json;
using API.Routes;
using Application.DTOs.Accounts;
using Application.DTOs.Common;
using Application.Services;
using Domain.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

public class AccountRoutesTests
{
    private readonly Mock<IAccountService> _mockService;
    private static readonly DateTime TestDate = DateTime.UtcNow;

    public AccountRoutesTests()
    {
        _mockService = new Mock<IAccountService>();
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
                    app.UseEndpoints(endpoints => endpoints.MapAccountRoutes());
                });
            })
            .Build();
    }

    [Fact]
    public async Task PostAccount_ReturnsCreated_WhenSuccessful()
    {
        var request = new CreateAccountRequest("Bank Account", AccountType.Checking, 1000m);
        var response = new AccountResponse(1, "Bank Account", AccountType.Checking, 1000m, TestDate, TestDate);
        _mockService.Setup(s => s.CreateAccountAsync(It.IsAny<CreateAccountRequest>()))
            .ReturnsAsync(Result<AccountResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PostAsJsonAsync("/api/v1/accounts", request);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Contains("/api/v1/accounts/1", httpResponse.Headers.Location?.ToString() ?? "");
    }

    [Fact]
    public async Task GetAccountById_ReturnsOk_WhenFound()
    {
        var response = new AccountResponse(1, "Bank Account", AccountType.Checking, 1000m, TestDate, TestDate);
        _mockService.Setup(s => s.GetAccountByIdAsync(1))
            .ReturnsAsync(Result<AccountResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/accounts/1");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        var result = await httpResponse.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.Equal("Bank Account", result?.Name);
    }

    [Fact]
    public async Task GetAccountById_ReturnsNotFound_WhenNotExists()
    {
        _mockService.Setup(s => s.GetAccountByIdAsync(999))
            .ReturnsAsync(Result<AccountResponse>.Failure("Account not found"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/accounts/999");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetAccounts_ReturnsOk_WithPaginatedList()
    {
        var accounts = new List<AccountResponse>
        {
            new(1, "Bank Account", AccountType.Checking, 1000m, TestDate, TestDate),
            new(2, "Cash", AccountType.Cash, 500m, TestDate, TestDate)
        };
        var paginatedList = new PaginatedList<AccountResponse>(accounts, 2, 1, 50);
        _mockService.Setup(s => s.GetAllAccountsAsync(1, 50))
            .ReturnsAsync(Result<PaginatedList<AccountResponse>>.Success(paginatedList));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/accounts?page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task PutAccount_ReturnsOk_WhenSuccessful()
    {
        var request = new UpdateAccountRequest("Updated Bank", AccountType.Checking, 2000m);
        var response = new AccountResponse(1, "Updated Bank", AccountType.Checking, 2000m, TestDate, TestDate);
        _mockService.Setup(s => s.UpdateAccountAsync(1, It.IsAny<UpdateAccountRequest>()))
            .ReturnsAsync(Result<AccountResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PutAsJsonAsync("/api/v1/accounts/1", request);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_ReturnsOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.DeleteAccountAsync(1))
            .ReturnsAsync(Result.Success());

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.DeleteAsync("/api/v1/accounts/1");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }
}
