//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Common;
using Application.DTOs.Incomes;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

public class IncomeServiceTests
{
    private readonly Mock<IIncomeRepository> _incomeRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<ILogger<IncomeService>> _loggerMock;
    private readonly IncomeService _service;

    public IncomeServiceTests()
    {
        _incomeRepositoryMock = new Mock<IIncomeRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILogger<IncomeService>>();

        _service = new IncomeService(
            _incomeRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _accountRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateIncomeAsync_ReturnsFailure_WhenCategoryIdInvalid()
    {
        CreateIncomeRequest request = new(100m, DateTime.UtcNow, 999, 1, "Test", string.Empty);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync(Result<Category>.Failure("Category not found"));

        Result<IncomeResponse> result = await _service.CreateIncomeAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Contains("Category not found", result.Error);
    }

    [Fact]
    public async Task CreateIncomeAsync_ReturnsSuccess_WhenAllFieldsValid()
    {
        DateTime date = DateTime.UtcNow;
        CreateIncomeRequest request = new(5000m, date, 1, 1, "Salary", "Monthly salary");

        Category category = new()
        {
            Id = 1,
            Name = "Salary",
            Type = TransactionType.Income,
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

        Income income = new()
        {
            Id = 1,
            Amount = 5000m,
            Date = date,
            CategoryId = 1,
            Category = category,
            AccountId = 1,
            Account = account,
            Description = "Salary",
            Notes = "Monthly salary",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Category>.Success(category));

        _accountRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Account>.Success(account));

        _incomeRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Income>()))
            .ReturnsAsync(Result<Income>.Success(income));

        _incomeRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        Result<IncomeResponse> result = await _service.CreateIncomeAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(5000m, result.Value.Amount);
        Assert.Equal("Salary", result.Value.CategoryName);
        Assert.Equal("Bank Account", result.Value.AccountName);
    }

    [Fact]
    public async Task GetIncomesAsync_FiltersBy_DateRange()
    {
        DateTime startDate = new(2025, 1, 1);
        DateTime endDate = new(2025, 1, 31);
        IncomeFilterParams filters = new(startDate, endDate, null, null, 1, 50);

        List<Income> incomes =
        [
            new()
            {
                Id = 1,
                Amount = 5000m,
                Date = new DateTime(2025, 1, 15),
                CategoryId = 1,
                Category = new() { Id = 1, Name = "Salary", Type = TransactionType.Income },
                AccountId = 1,
                Account = new() { Id = 1, Name = "Bank", Type = AccountType.Checking },
                Description = "Test",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ];

        _incomeRepositoryMock
            .Setup(x => x.GetFilteredAsync(startDate, endDate, null, null, 1, 50))
            .ReturnsAsync(Result<IEnumerable<Income>>.Success(incomes));

        _incomeRepositoryMock
            .Setup(x => x.GetTotalCountAsync(startDate, endDate, null, null))
            .ReturnsAsync(Result<int>.Success(1));

        Result<PaginatedList<IncomeResponse>> result = await _service.GetIncomesAsync(filters);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
    }

    [Fact]
    public async Task IncomeResponseIncludesCategoryNameAndAccountName()
    {
        Category category = new()
        {
            Id = 1,
            Name = "Salary",
            Type = TransactionType.Income,
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

        Income income = new()
        {
            Id = 1,
            Amount = 5000m,
            Date = DateTime.UtcNow,
            CategoryId = 1,
            Category = category,
            AccountId = 1,
            Account = account,
            Description = "Monthly salary",
            Notes = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _incomeRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Income>.Success(income));

        Result<IncomeResponse> result = await _service.GetIncomeByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Salary", result.Value!.CategoryName);
        Assert.Equal("Checking Account", result.Value.AccountName);
    }
}
