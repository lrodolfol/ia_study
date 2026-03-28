//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Common;
using Application.DTOs.ExpenseItems;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

public class ExpenseItemServiceTests
{
    private readonly Mock<IExpenseItemRepository> _expenseItemRepositoryMock;
    private readonly Mock<IExpenseRepository> _expenseRepositoryMock;
    private readonly Mock<ILogger<ExpenseItemService>> _loggerMock;
    private readonly ExpenseItemService _service;

    public ExpenseItemServiceTests()
    {
        _expenseItemRepositoryMock = new Mock<IExpenseItemRepository>();
        _expenseRepositoryMock = new Mock<IExpenseRepository>();
        _loggerMock = new Mock<ILogger<ExpenseItemService>>();

        _service = new ExpenseItemService(
            _expenseItemRepositoryMock.Object,
            _expenseRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateExpenseItemAsync_ReturnsFailure_WhenExpenseIdInvalid()
    {
        CreateExpenseItemRequest request = new(999, 50m, "Test item", null);

        _expenseRepositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync(Result<Expense>.Failure("Expense not found"));

        Result<ExpenseItemResponse> result = await _service.CreateExpenseItemAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Contains("Expense not found", result.Error);
    }

    [Fact]
    public async Task CreateExpenseItemAsync_ReturnsFailure_WhenSumExceedsParentAmount()
    {
        CreateExpenseItemRequest request = new(1, 60m, "Test item", null);

        Expense expense = new()
        {
            Id = 1,
            Amount = 100m,
            Date = DateTime.UtcNow,
            CategoryId = 1,
            AccountId = 1,
            Description = "Parent expense",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _expenseRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Expense>.Success(expense));

        _expenseItemRepositoryMock
            .Setup(x => x.GetSumByExpenseIdAsync(1))
            .ReturnsAsync(Result<decimal>.Success(50m));

        Result<ExpenseItemResponse> result = await _service.CreateExpenseItemAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Contains("Sum of expense items cannot exceed parent expense amount", result.Error);
    }

    [Fact]
    public async Task CreateExpenseItemAsync_ReturnsSuccess_WhenSumValid()
    {
        CreateExpenseItemRequest request = new(1, 30m, "Test item", null);

        Expense expense = new()
        {
            Id = 1,
            Amount = 100m,
            Date = DateTime.UtcNow,
            CategoryId = 1,
            AccountId = 1,
            Description = "Parent expense",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        ExpenseItem expenseItem = new()
        {
            Id = 1,
            ExpenseId = 1,
            Amount = 30m,
            Description = "Test item",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _expenseRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Expense>.Success(expense));

        _expenseItemRepositoryMock
            .Setup(x => x.GetSumByExpenseIdAsync(1))
            .ReturnsAsync(Result<decimal>.Success(50m));

        _expenseItemRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ExpenseItem>()))
            .ReturnsAsync(Result<ExpenseItem>.Success(expenseItem));

        _expenseItemRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        Result<ExpenseItemResponse> result = await _service.CreateExpenseItemAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(30m, result.Value.Amount);
    }

    [Fact]
    public async Task UpdateExpenseItemAsync_ValidatesSumAfterUpdate()
    {
        UpdateExpenseItemRequest request = new(80m, null, null);

        Expense expense = new()
        {
            Id = 1,
            Amount = 100m,
            Date = DateTime.UtcNow,
            CategoryId = 1,
            AccountId = 1,
            Description = "Parent expense",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        ExpenseItem existingItem = new()
        {
            Id = 1,
            ExpenseId = 1,
            Expense = expense,
            Amount = 30m,
            Description = "Test item",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _expenseItemRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<ExpenseItem>.Success(existingItem));

        _expenseRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Expense>.Success(expense));

        _expenseItemRepositoryMock
            .Setup(x => x.GetSumByExpenseIdAsync(1))
            .ReturnsAsync(Result<decimal>.Success(60m));

        Result<ExpenseItemResponse> result = await _service.UpdateExpenseItemAsync(1, request);

        Assert.False(result.IsSuccess);
        Assert.Contains("Sum of expense items cannot exceed parent expense amount", result.Error);
    }

    [Fact]
    public async Task GetExpenseItemsByExpenseIdAsync_ReturnsItemsForSpecificExpense()
    {
        List<ExpenseItem> items =
        [
            new()
            {
                Id = 1,
                ExpenseId = 1,
                Amount = 30m,
                Description = "Item 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                ExpenseId = 1,
                Amount = 20m,
                Description = "Item 2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ];

        _expenseItemRepositoryMock
            .Setup(x => x.GetByExpenseIdAsync(1, 1, 50))
            .ReturnsAsync(Result<IEnumerable<ExpenseItem>>.Success(items));

        _expenseItemRepositoryMock
            .Setup(x => x.GetTotalCountByExpenseAsync(1))
            .ReturnsAsync(Result<int>.Success(2));

        Result<PaginatedList<ExpenseItemResponse>> result = await _service.GetExpenseItemsByExpenseIdAsync(1, 1, 50);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Items.Count);
        Assert.All(result.Value.Items, item => Assert.Equal(1, item.ExpenseId));
    }

    [Fact]
    public async Task CreateMultipleExpenseItems_ValidatesCumulativeSum()
    {
        Expense expense = new()
        {
            Id = 1,
            Amount = 100m,
            Date = DateTime.UtcNow,
            CategoryId = 1,
            AccountId = 1,
            Description = "Parent expense",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        ExpenseItem firstItem = new()
        {
            Id = 1,
            ExpenseId = 1,
            Amount = 40m,
            Description = "First item",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _expenseRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Expense>.Success(expense));

        _expenseItemRepositoryMock
            .Setup(x => x.GetSumByExpenseIdAsync(1))
            .ReturnsAsync(Result<decimal>.Success(0m));

        _expenseItemRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ExpenseItem>()))
            .ReturnsAsync(Result<ExpenseItem>.Success(firstItem));

        _expenseItemRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        CreateExpenseItemRequest firstRequest = new(1, 40m, "First item", null);
        Result<ExpenseItemResponse> firstResult = await _service.CreateExpenseItemAsync(firstRequest);

        Assert.True(firstResult.IsSuccess);

        _expenseItemRepositoryMock
            .Setup(x => x.GetSumByExpenseIdAsync(1))
            .ReturnsAsync(Result<decimal>.Success(40m));

        CreateExpenseItemRequest secondRequest = new(1, 70m, "Second item", null);
        Result<ExpenseItemResponse> secondResult = await _service.CreateExpenseItemAsync(secondRequest);

        Assert.False(secondResult.IsSuccess);
        Assert.Contains("Sum of expense items cannot exceed parent expense amount", secondResult.Error);
    }
}
