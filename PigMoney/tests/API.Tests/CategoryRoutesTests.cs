//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Tests;

using System.Net;
using System.Net.Http.Json;
using API.Routes;
using Application.DTOs.Categories;
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

public class CategoryRoutesTests
{
    private readonly Mock<ICategoryService> _mockService;
    private static readonly DateTime TestDate = DateTime.UtcNow;

    public CategoryRoutesTests()
    {
        _mockService = new Mock<ICategoryService>();
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
                    app.UseEndpoints(endpoints => endpoints.MapCategoryRoutes());
                });
            })
            .Build();
    }

    [Fact]
    public async Task PostCategory_ReturnsCreated_WhenSuccessful()
    {
        var request = new CreateCategoryRequest("Food", TransactionType.Expense);
        var response = new CategoryResponse(1, "Food", TransactionType.Expense, TestDate, TestDate);
        _mockService.Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryRequest>()))
            .ReturnsAsync(Result<CategoryResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PostAsJsonAsync("/api/v1/categories", request);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Contains("/api/v1/categories/1", httpResponse.Headers.Location?.ToString() ?? "");
    }

    [Fact]
    public async Task PostCategory_ReturnsBadRequest_WhenServiceFails()
    {
        var request = new CreateCategoryRequest("", TransactionType.Expense);
        _mockService.Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryRequest>()))
            .ReturnsAsync(Result<CategoryResponse>.Failure("Name is required"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PostAsJsonAsync("/api/v1/categories", request);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsOk_WhenFound()
    {
        var response = new CategoryResponse(1, "Food", TransactionType.Expense, TestDate, TestDate);
        _mockService.Setup(s => s.GetCategoryByIdAsync(1))
            .ReturnsAsync(Result<CategoryResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/categories/1");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        var result = await httpResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.Equal("Food", result?.Name);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsNotFound_WhenNotExists()
    {
        _mockService.Setup(s => s.GetCategoryByIdAsync(999))
            .ReturnsAsync(Result<CategoryResponse>.Failure("Category not found"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/categories/999");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetCategories_ReturnsOk_WithPaginatedList()
    {
        var categories = new List<CategoryResponse>
        {
            new(1, "Food", TransactionType.Expense, TestDate, TestDate),
            new(2, "Transport", TransactionType.Expense, TestDate, TestDate)
        };
        var paginatedList = new PaginatedList<CategoryResponse>(categories, 2, 1, 50);
        _mockService.Setup(s => s.GetAllCategoriesAsync(1, 50))
            .ReturnsAsync(Result<PaginatedList<CategoryResponse>>.Success(paginatedList));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.GetAsync("/api/v1/categories?page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task PutCategory_ReturnsOk_WhenSuccessful()
    {
        var request = new UpdateCategoryRequest("Updated Food", TransactionType.Expense);
        var response = new CategoryResponse(1, "Updated Food", TransactionType.Expense, TestDate, TestDate);
        _mockService.Setup(s => s.UpdateCategoryAsync(1, It.IsAny<UpdateCategoryRequest>()))
            .ReturnsAsync(Result<CategoryResponse>.Success(response));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PutAsJsonAsync("/api/v1/categories/1", request);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task PutCategory_ReturnsNotFound_WhenNotExists()
    {
        var request = new UpdateCategoryRequest("Updated", TransactionType.Expense);
        _mockService.Setup(s => s.UpdateCategoryAsync(999, It.IsAny<UpdateCategoryRequest>()))
            .ReturnsAsync(Result<CategoryResponse>.Failure("Category not found"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.PutAsJsonAsync("/api/v1/categories/999", request);

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.DeleteCategoryAsync(1))
            .ReturnsAsync(Result.Success());

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.DeleteAsync("/api/v1/categories/1");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsBadRequest_WhenHasDependencies()
    {
        _mockService.Setup(s => s.DeleteCategoryAsync(1))
            .ReturnsAsync(Result.Failure("Cannot delete category with existing expenses"));

        using var host = CreateTestHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var httpResponse = await client.DeleteAsync("/api/v1/categories/1");

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }
}
