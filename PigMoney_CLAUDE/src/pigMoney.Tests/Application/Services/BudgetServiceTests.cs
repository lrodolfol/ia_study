//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Budgets;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace pigMoney.Tests.Application.Services;

public class BudgetServiceTests
{
    private readonly Mock<IBudgetRepository> _budgetRepositoryMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<ILogger<BudgetService>> _loggerMock = new();
    private readonly BudgetService _service;

    public BudgetServiceTests()
    {
        _service = new BudgetService(_budgetRepositoryMock.Object, _categoryRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess()
    {
        var request = new CreateBudgetRequest(1, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 5000m);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _budgetRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Budget>()))
            .ReturnsAsync((Budget b) => { b.Id = 1; return b; });

        var result = await _service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(5000m, result.Value!.LimitAmount);
        Assert.Equal(1, result.Value.CategoryId);
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryNotFound_ShouldReturnFailure()
    {
        var request = new CreateBudgetRequest(99, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 5000m);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        var result = await _service.CreateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_ShouldReturnSuccess()
    {
        var budget = new Budget { Id = 1, CategoryId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1), LimitAmount = 3000m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _budgetRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(budget);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        var result = await _service.UpdateAsync(1, new UpdateBudgetRequest(1, DateTime.UtcNow, DateTime.UtcNow.AddMonths(2), 6000m));

        Assert.True(result.IsSuccess);
        Assert.Equal(6000m, result.Value!.LimitAmount);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldReturnFailure()
    {
        _budgetRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Budget?)null);

        var result = await _service.UpdateAsync(99, new UpdateBudgetRequest(1, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 5000m));

        Assert.False(result.IsSuccess);
        Assert.Equal("Budget not found.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryNotFound_ShouldReturnFailure()
    {
        var budget = new Budget { Id = 1, CategoryId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1), LimitAmount = 3000m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _budgetRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(budget);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        var result = await _service.UpdateAsync(1, new UpdateBudgetRequest(99, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 5000m));

        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_ShouldReturnSuccess()
    {
        var budget = new Budget { Id = 1, CategoryId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1), LimitAmount = 3000m };
        _budgetRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(budget);

        var result = await _service.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _budgetRepositoryMock.Verify(r => r.DeleteAsync(budget), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ShouldReturnFailure()
    {
        _budgetRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Budget?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("Budget not found.", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnFailure()
    {
        _budgetRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Budget?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("Budget not found.", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ShouldReturnSuccess()
    {
        var budget = new Budget { Id = 1, CategoryId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1), LimitAmount = 2000m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _budgetRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(budget);

        var result = await _service.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Id);
        Assert.Equal(2000m, result.Value.LimitAmount);
    }

    [Fact]
    public async Task GetAllAsync_WithFilters_ShouldReturnFilteredResults()
    {
        var filters = new BudgetFilterParams(1, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        _budgetRepositoryMock.Setup(r => r.GetFilteredAsync(It.IsAny<BudgetFilterParams>(), 1, 10))
            .ReturnsAsync([]);
        _budgetRepositoryMock.Setup(r => r.CountFilteredAsync(It.IsAny<BudgetFilterParams>()))
            .ReturnsAsync(0);

        var result = await _service.GetAllAsync(filters, 1, 10);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_NoFilters_ShouldReturnAllPaginated()
    {
        var filters = new BudgetFilterParams(null, null, null);
        var budgets = new List<Budget>
        {
            new() { Id = 1, CategoryId = 1, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1), LimitAmount = 1000m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _budgetRepositoryMock.Setup(r => r.GetFilteredAsync(It.IsAny<BudgetFilterParams>(), 1, 10))
            .ReturnsAsync(budgets);
        _budgetRepositoryMock.Setup(r => r.CountFilteredAsync(It.IsAny<BudgetFilterParams>()))
            .ReturnsAsync(1);

        var result = await _service.GetAllAsync(filters, 1, 10);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.TotalCount);
    }
}
