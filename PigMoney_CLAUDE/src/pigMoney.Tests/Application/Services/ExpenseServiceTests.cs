//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Expenses;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace pigMoney.Tests.Application.Services;

public class ExpenseServiceTests
{
    private readonly Mock<IExpenseRepository> _expenseRepositoryMock = new();
    private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<IExpenseItemRepository> _expenseItemRepositoryMock = new();
    private readonly Mock<ILogger<ExpenseService>> _loggerMock = new();
    private readonly ExpenseService _service;

    public ExpenseServiceTests()
    {
        _service = new ExpenseService(
            _expenseRepositoryMock.Object,
            _accountRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _expenseItemRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess()
    {
        var request = new CreateExpenseRequest(250m, DateTime.UtcNow, "Groceries", 1, 2);
        _accountRepositoryMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(2)).ReturnsAsync(true);
        _expenseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Expense>()))
            .ReturnsAsync((Expense e) => { e.Id = 1; return e; });

        var result = await _service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(250m, result.Value!.Amount);
        Assert.Equal("Groceries", result.Value.Description);
        Assert.Equal(1, result.Value.AccountId);
        Assert.Equal(2, result.Value.CategoryId);
    }

    [Fact]
    public async Task CreateAsync_WhenAccountNotFound_ShouldReturnFailure()
    {
        var request = new CreateExpenseRequest(250m, DateTime.UtcNow, "Groceries", 99, 2);
        _accountRepositoryMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        var result = await _service.CreateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryNotFound_ShouldReturnFailure()
    {
        var request = new CreateExpenseRequest(250m, DateTime.UtcNow, "Groceries", 1, 99);
        _accountRepositoryMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        var result = await _service.CreateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_ShouldReturnSuccess()
    {
        var expense = new Expense { Id = 1, Amount = 100m, Date = DateTime.UtcNow, Description = "Old", AccountId = 1, CategoryId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _expenseRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expense);
        _accountRepositoryMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(2)).ReturnsAsync(true);

        var result = await _service.UpdateAsync(1, new UpdateExpenseRequest(500m, DateTime.UtcNow, "Updated", 1, 2));

        Assert.True(result.IsSuccess);
        Assert.Equal(500m, result.Value!.Amount);
        Assert.Equal("Updated", result.Value.Description);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldReturnFailure()
    {
        _expenseRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Expense?)null);

        var result = await _service.UpdateAsync(99, new UpdateExpenseRequest(500m, DateTime.UtcNow, "Updated", 1, 2));

        Assert.False(result.IsSuccess);
        Assert.Equal("Expense not found.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenAccountNotFound_ShouldReturnFailure()
    {
        var expense = new Expense { Id = 1, Amount = 100m, Date = DateTime.UtcNow, Description = "Test", AccountId = 1, CategoryId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _expenseRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expense);
        _accountRepositoryMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        var result = await _service.UpdateAsync(1, new UpdateExpenseRequest(500m, DateTime.UtcNow, "Updated", 99, 2));

        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryNotFound_ShouldReturnFailure()
    {
        var expense = new Expense { Id = 1, Amount = 100m, Date = DateTime.UtcNow, Description = "Test", AccountId = 1, CategoryId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _expenseRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expense);
        _accountRepositoryMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        var result = await _service.UpdateAsync(1, new UpdateExpenseRequest(500m, DateTime.UtcNow, "Updated", 1, 99));

        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_ShouldReturnSuccess()
    {
        var expense = new Expense { Id = 1, Amount = 100m, Date = DateTime.UtcNow, Description = "Test", AccountId = 1, CategoryId = 2 };
        _expenseRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expense);
        _expenseItemRepositoryMock.Setup(r => r.CountByExpenseIdAsync(1)).ReturnsAsync(0);

        var result = await _service.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _expenseRepositoryMock.Verify(r => r.DeleteAsync(expense), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenHasItems_ShouldReturn409Failure()
    {
        var expense = new Expense { Id = 1, Amount = 100m, Date = DateTime.UtcNow, Description = "Test", AccountId = 1, CategoryId = 2 };
        _expenseRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expense);
        _expenseItemRepositoryMock.Setup(r => r.CountByExpenseIdAsync(1)).ReturnsAsync(3);

        var result = await _service.DeleteAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot delete expense with existing items.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ShouldReturnFailure()
    {
        _expenseRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Expense?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("Expense not found.", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnFailure()
    {
        _expenseRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Expense?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("Expense not found.", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ShouldReturnSuccess()
    {
        var expense = new Expense { Id = 1, Amount = 300m, Date = DateTime.UtcNow, Description = "Rent", AccountId = 1, CategoryId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _expenseRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expense);

        var result = await _service.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Id);
        Assert.Equal(300m, result.Value.Amount);
    }

    [Fact]
    public async Task GetAllAsync_WithFilters_ShouldReturnFilteredResults()
    {
        var filters = new ExpenseFilterParams(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, 1, 2);
        _expenseRepositoryMock.Setup(r => r.GetFilteredAsync(It.IsAny<ExpenseFilterParams>(), 1, 10))
            .ReturnsAsync([]);
        _expenseRepositoryMock.Setup(r => r.CountFilteredAsync(It.IsAny<ExpenseFilterParams>()))
            .ReturnsAsync(0);

        var result = await _service.GetAllAsync(filters, 1, 10);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_NoFilters_ShouldReturnAllPaginated()
    {
        var filters = new ExpenseFilterParams(null, null, null, null);
        var expenses = new List<Expense>
        {
            new() { Id = 1, Amount = 100m, Date = DateTime.UtcNow, Description = "Expense 1", AccountId = 1, CategoryId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _expenseRepositoryMock.Setup(r => r.GetFilteredAsync(It.IsAny<ExpenseFilterParams>(), 1, 10))
            .ReturnsAsync(expenses);
        _expenseRepositoryMock.Setup(r => r.CountFilteredAsync(It.IsAny<ExpenseFilterParams>()))
            .ReturnsAsync(1);

        var result = await _service.GetAllAsync(filters, 1, 10);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.TotalCount);
    }
}
