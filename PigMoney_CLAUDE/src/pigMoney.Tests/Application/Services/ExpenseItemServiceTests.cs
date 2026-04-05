//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.ExpenseItems;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace pigMoney.Tests.Application.Services;

public class ExpenseItemServiceTests
{
    private readonly Mock<IExpenseItemRepository> _expenseItemRepositoryMock = new();
    private readonly Mock<IExpenseRepository> _expenseRepositoryMock = new();
    private readonly Mock<ILogger<ExpenseItemService>> _loggerMock = new();
    private readonly ExpenseItemService _service;

    public ExpenseItemServiceTests()
    {
        _service = new ExpenseItemService(
            _expenseItemRepositoryMock.Object,
            _expenseRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess()
    {
        var request = new CreateExpenseItemRequest("Rice", 2m, 5.50m);
        _expenseRepositoryMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _expenseItemRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ExpenseItem>()))
            .ReturnsAsync((ExpenseItem ei) => { ei.Id = 1; return ei; });

        var result = await _service.CreateAsync(1, request);

        Assert.True(result.IsSuccess);
        Assert.Equal("Rice", result.Value!.Name);
        Assert.Equal(2m, result.Value.Quantity);
        Assert.Equal(5.50m, result.Value.UnitPrice);
        Assert.Equal(1, result.Value.ExpenseId);
    }

    [Fact]
    public async Task CreateAsync_WhenExpenseNotFound_ShouldReturnFailure()
    {
        var request = new CreateExpenseItemRequest("Rice", 2m, 5.50m);
        _expenseRepositoryMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        var result = await _service.CreateAsync(99, request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Expense not found.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_ShouldReturnSuccess()
    {
        var item = new ExpenseItem { Id = 1, Name = "Rice", Quantity = 2m, UnitPrice = 5.50m, ExpenseId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _expenseItemRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);

        var result = await _service.UpdateAsync(1, new UpdateExpenseItemRequest("Beans", 3m, 4.00m));

        Assert.True(result.IsSuccess);
        Assert.Equal("Beans", result.Value!.Name);
        Assert.Equal(3m, result.Value.Quantity);
        Assert.Equal(4.00m, result.Value.UnitPrice);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldReturnFailure()
    {
        _expenseItemRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ExpenseItem?)null);

        var result = await _service.UpdateAsync(99, new UpdateExpenseItemRequest("Beans", 3m, 4.00m));

        Assert.False(result.IsSuccess);
        Assert.Equal("Expense item not found.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_ShouldReturnSuccess()
    {
        var item = new ExpenseItem { Id = 1, Name = "Rice", Quantity = 2m, UnitPrice = 5.50m, ExpenseId = 1 };
        _expenseItemRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);

        var result = await _service.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _expenseItemRepositoryMock.Verify(r => r.DeleteAsync(item), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ShouldReturnFailure()
    {
        _expenseItemRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ExpenseItem?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("Expense item not found.", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnFailure()
    {
        _expenseItemRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ExpenseItem?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("Expense item not found.", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ShouldReturnSuccess()
    {
        var item = new ExpenseItem { Id = 1, Name = "Rice", Quantity = 2m, UnitPrice = 5.50m, ExpenseId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _expenseItemRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);

        var result = await _service.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Id);
        Assert.Equal("Rice", result.Value.Name);
    }

    [Fact]
    public async Task GetAllByExpenseIdAsync_ShouldReturnPaginatedResults()
    {
        var items = new List<ExpenseItem>
        {
            new() { Id = 1, Name = "Rice", Quantity = 2m, UnitPrice = 5.50m, ExpenseId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _expenseItemRepositoryMock.Setup(r => r.GetByExpenseIdAsync(1, 1, 10)).ReturnsAsync(items);
        _expenseItemRepositoryMock.Setup(r => r.CountByExpenseIdAsync(1)).ReturnsAsync(1);

        var result = await _service.GetAllByExpenseIdAsync(1, 1, 10);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetAllByExpenseIdAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        _expenseItemRepositoryMock.Setup(r => r.GetByExpenseIdAsync(1, 1, 10)).ReturnsAsync([]);
        _expenseItemRepositoryMock.Setup(r => r.CountByExpenseIdAsync(1)).ReturnsAsync(0);

        var result = await _service.GetAllByExpenseIdAsync(1, 1, 10);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }
}
