//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Common;
using Application.DTOs.Expenses;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

public class ExpenseServiceTests
{
    private readonly Mock<IExpenseRepository> _expenseRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<ILogger<ExpenseService>> _loggerMock;
    private readonly ExpenseService _service;

    public ExpenseServiceTests()
    {
        _expenseRepositoryMock = new Mock<IExpenseRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILogger<ExpenseService>>();

        _service = new ExpenseService(
            _expenseRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _accountRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateExpenseAsync_ReturnsFailure_WhenCategoryIdInvalid()
    {
        CreateExpenseRequest request = new(100m, DateTime.UtcNow, 999, 1, "Test", string.Empty);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync(Result<Category>.Failure("Category not found"));

        Result<ExpenseResponse> result = await _service.CreateExpenseAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Contains("Category not found", result.Error);
    }

    [Fact]
    public async Task CreateExpenseAsync_ReturnsFailure_WhenAccountIdInvalid()
    {
        CreateExpenseRequest request = new(100m, DateTime.UtcNow, 1, 999, "Test", string.Empty);

        Category category = new()
        {
            Id = 1,
            Name = "Food",
            Type = TransactionType.Expense,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Category>.Success(category));

        _accountRepositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync(Result<Account>.Failure("Account not found"));

        Result<ExpenseResponse> result = await _service.CreateExpenseAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Contains("Account not found", result.Error);
    }

    [Fact]
    public async Task CreateExpenseAsync_ReturnsSuccess_WhenAllFieldsValid()
    {
        DateTime date = DateTime.UtcNow;
        CreateExpenseRequest request = new(100m, date, 1, 1, "Groceries", "Weekly shopping");

        Category category = new()
        {
            Id = 1,
            Name = "Food",
            Type = TransactionType.Expense,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Account account = new()
        {
            Id = 1,
            Name = "Bank Account",
            Type = AccountType.Checking,
            InitialBalance = 1000m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Expense expense = new()
        {
            Id = 1,
            Amount = 100m,
            Date = date,
            CategoryId = 1,
            Category = category,
            AccountId = 1,
            Account = account,
            Description = "Groceries",
            Notes = "Weekly shopping",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Category>.Success(category));

        _accountRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Account>.Success(account));

        _expenseRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Expense>()))
            .ReturnsAsync(Result<Expense>.Success(expense));

        _expenseRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        Result<ExpenseResponse> result = await _service.CreateExpenseAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(100m, result.Value.Amount);
        Assert.Equal("Food", result.Value.CategoryName);
        Assert.Equal("Bank Account", result.Value.AccountName);
    }

    [Fact]
    public async Task GetExpensesAsync_FiltersBy_DateRange()
    {
        DateTime startDate = new(2025, 1, 1);
        DateTime endDate = new(2025, 1, 31);
        ExpenseFilterParams filters = new(startDate, endDate, null, null, 1, 50);

        List<Expense> expenses =
        [
            new()
            {
                Id = 1,
                Amount = 100m,
                Date = new DateTime(2025, 1, 15),
                CategoryId = 1,
                Category = new() { Id = 1, Name = "Food", Type = TransactionType.Expense },
                AccountId = 1,
                Account = new() { Id = 1, Name = "Bank", Type = AccountType.Checking },
                Description = "Test",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ];

        _expenseRepositoryMock
            .Setup(x => x.GetFilteredAsync(startDate, endDate, null, null, 1, 50))
            .ReturnsAsync(Result<IEnumerable<Expense>>.Success(expenses));

        _expenseRepositoryMock
            .Setup(x => x.GetTotalCountAsync(startDate, endDate, null, null))
            .ReturnsAsync(Result<int>.Success(1));

        Result<PaginatedList<ExpenseResponse>> result = await _service.GetExpensesAsync(filters);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
    }

    [Fact]
    public async Task GetExpensesAsync_FiltersBy_CategoryId()
    {
        ExpenseFilterParams filters = new(null, null, 1, null, 1, 50);

        List<Expense> expenses =
        [
            new()
            {
                Id = 1,
                Amount = 100m,
                Date = DateTime.UtcNow,
                CategoryId = 1,
                Category = new() { Id = 1, Name = "Food", Type = TransactionType.Expense },
                AccountId = 1,
                Account = new() { Id = 1, Name = "Bank", Type = AccountType.Checking },
                Description = "Test",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ];

        _expenseRepositoryMock
            .Setup(x => x.GetFilteredAsync(null, null, 1, null, 1, 50))
            .ReturnsAsync(Result<IEnumerable<Expense>>.Success(expenses));

        _expenseRepositoryMock
            .Setup(x => x.GetTotalCountAsync(null, null, 1, null))
            .ReturnsAsync(Result<int>.Success(1));

        Result<PaginatedList<ExpenseResponse>> result = await _service.GetExpensesAsync(filters);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.Items[0].CategoryId);
    }

    [Fact]
    public async Task GetExpensesAsync_FiltersBy_AccountId()
    {
        ExpenseFilterParams filters = new(null, null, null, 2, 1, 50);

        List<Expense> expenses =
        [
            new()
            {
                Id = 1,
                Amount = 100m,
                Date = DateTime.UtcNow,
                CategoryId = 1,
                Category = new() { Id = 1, Name = "Food", Type = TransactionType.Expense },
                AccountId = 2,
                Account = new() { Id = 2, Name = "Savings", Type = AccountType.Savings },
                Description = "Test",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ];

        _expenseRepositoryMock
            .Setup(x => x.GetFilteredAsync(null, null, null, 2, 1, 50))
            .ReturnsAsync(Result<IEnumerable<Expense>>.Success(expenses));

        _expenseRepositoryMock
            .Setup(x => x.GetTotalCountAsync(null, null, null, 2))
            .ReturnsAsync(Result<int>.Success(1));

        Result<PaginatedList<ExpenseResponse>> result = await _service.GetExpensesAsync(filters);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(2, result.Value.Items[0].AccountId);
    }

    [Fact]
    public async Task GetExpensesAsync_CombinesMultipleFilters()
    {
        DateTime startDate = new(2025, 1, 1);
        DateTime endDate = new(2025, 1, 31);
        ExpenseFilterParams filters = new(startDate, endDate, 1, null, 1, 50);

        List<Expense> expenses =
        [
            new()
            {
                Id = 1,
                Amount = 100m,
                Date = new DateTime(2025, 1, 15),
                CategoryId = 1,
                Category = new() { Id = 1, Name = "Food", Type = TransactionType.Expense },
                AccountId = 1,
                Account = new() { Id = 1, Name = "Bank", Type = AccountType.Checking },
                Description = "Test",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ];

        _expenseRepositoryMock
            .Setup(x => x.GetFilteredAsync(startDate, endDate, 1, null, 1, 50))
            .ReturnsAsync(Result<IEnumerable<Expense>>.Success(expenses));

        _expenseRepositoryMock
            .Setup(x => x.GetTotalCountAsync(startDate, endDate, 1, null))
            .ReturnsAsync(Result<int>.Success(1));

        Result<PaginatedList<ExpenseResponse>> result = await _service.GetExpensesAsync(filters);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
    }

    [Fact]
    public async Task ExpenseResponseIncludesCategoryNameAndAccountName()
    {
        Category category = new()
        {
            Id = 1,
            Name = "Food",
            Type = TransactionType.Expense,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Account account = new()
        {
            Id = 1,
            Name = "Checking Account",
            Type = AccountType.Checking,
            InitialBalance = 1000m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Expense expense = new()
        {
            Id = 1,
            Amount = 100m,
            Date = DateTime.UtcNow,
            CategoryId = 1,
            Category = category,
            AccountId = 1,
            Account = account,
            Description = "Groceries",
            Notes = "Weekly shopping",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _expenseRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Expense>.Success(expense));

        Result<ExpenseResponse> result = await _service.GetExpenseByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Food", result.Value!.CategoryName);
        Assert.Equal("Checking Account", result.Value.AccountName);
    }

    [Fact]
    public async Task GetExpensesAsync_WithPagination_ReturnsCorrectPage()
    {
        ExpenseFilterParams filters = new(null, null, null, null, 2, 10);

        List<Expense> expenses = Enumerable.Range(11, 10).Select(i => new Expense
        {
            Id = i,
            Amount = i * 10m,
            Date = DateTime.UtcNow,
            CategoryId = 1,
            Category = new() { Id = 1, Name = "Food", Type = TransactionType.Expense },
            AccountId = 1,
            Account = new() { Id = 1, Name = "Bank", Type = AccountType.Checking },
            Description = $"Expense {i}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        _expenseRepositoryMock
            .Setup(x => x.GetFilteredAsync(null, null, null, null, 2, 10))
            .ReturnsAsync(Result<IEnumerable<Expense>>.Success(expenses));

        _expenseRepositoryMock
            .Setup(x => x.GetTotalCountAsync(null, null, null, null))
            .ReturnsAsync(Result<int>.Success(100));

        Result<PaginatedList<ExpenseResponse>> result = await _service.GetExpensesAsync(filters);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value!.Items.Count);
        Assert.Equal(100, result.Value.TotalCount);
        Assert.Equal(10, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Page);
    }

    [Fact]
    public async Task GetExpensesAsync_WithNoMatches_ReturnsEmptyList()
    {
        ExpenseFilterParams filters = new(null, null, 999, null, 1, 50);

        _expenseRepositoryMock
            .Setup(x => x.GetFilteredAsync(null, null, 999, null, 1, 50))
            .ReturnsAsync(Result<IEnumerable<Expense>>.Success(Enumerable.Empty<Expense>()));

        _expenseRepositoryMock
            .Setup(x => x.GetTotalCountAsync(null, null, 999, null))
            .ReturnsAsync(Result<int>.Success(0));

        Result<PaginatedList<ExpenseResponse>> result = await _service.GetExpensesAsync(filters);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }
}
