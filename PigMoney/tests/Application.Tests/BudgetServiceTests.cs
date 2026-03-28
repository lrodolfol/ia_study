//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Budgets;
using Application.DTOs.Common;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

public class BudgetServiceTests
{
    private readonly Mock<IBudgetRepository> _budgetRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ILogger<BudgetService>> _loggerMock;
    private readonly BudgetService _service;

    public BudgetServiceTests()
    {
        _budgetRepositoryMock = new Mock<IBudgetRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _loggerMock = new Mock<ILogger<BudgetService>>();

        _service = new BudgetService(
            _budgetRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateBudgetAsync_ReturnsFailure_WhenCategoryIdInvalid()
    {
        CreateBudgetRequest request = new(999, 1000m, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync(Result<Category>.Failure("Category not found"));

        Result<BudgetResponse> result = await _service.CreateBudgetAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Contains("Category not found", result.Error);
    }

    [Fact]
    public async Task CreateBudgetAsync_ReturnsSuccess_WhenAllFieldsValid()
    {
        DateTime startDate = DateTime.UtcNow;
        DateTime endDate = startDate.AddMonths(1);
        CreateBudgetRequest request = new(1, 1000m, startDate, endDate);

        Category category = new()
        {
            Id = 1,
            Name = "Food",
            Type = TransactionType.Expense,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Budget budget = new()
        {
            Id = 1,
            CategoryId = 1,
            Category = category,
            LimitAmount = 1000m,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Category>.Success(category));

        _budgetRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Budget>()))
            .ReturnsAsync(Result<Budget>.Success(budget));

        _budgetRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        Result<BudgetResponse> result = await _service.CreateBudgetAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1000m, result.Value.LimitAmount);
        Assert.Equal("Food", result.Value.CategoryName);
    }

    [Fact]
    public async Task GetBudgetsByCategoryAsync_FiltersByCategoryCorrectly()
    {
        Category category = new()
        {
            Id = 1,
            Name = "Food",
            Type = TransactionType.Expense,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        List<Budget> budgets =
        [
            new()
            {
                Id = 1,
                CategoryId = 1,
                Category = category,
                LimitAmount = 1000m,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ];

        _budgetRepositoryMock
            .Setup(x => x.GetByCategoryIdAsync(1, 1, 50))
            .ReturnsAsync(Result<IEnumerable<Budget>>.Success(budgets));

        _budgetRepositoryMock
            .Setup(x => x.GetTotalCountByCategoryAsync(1))
            .ReturnsAsync(Result<int>.Success(1));

        Result<PaginatedList<BudgetResponse>> result = await _service.GetBudgetsByCategoryAsync(1, 1, 50);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.Items[0].CategoryId);
    }

    [Fact]
    public async Task GetBudgetsByCategoryAsync_ReturnsEmptyList_WhenNoBudgetsForCategory()
    {
        _budgetRepositoryMock
            .Setup(x => x.GetByCategoryIdAsync(999, 1, 50))
            .ReturnsAsync(Result<IEnumerable<Budget>>.Success(Enumerable.Empty<Budget>()));

        _budgetRepositoryMock
            .Setup(x => x.GetTotalCountByCategoryAsync(999))
            .ReturnsAsync(Result<int>.Success(0));

        Result<PaginatedList<BudgetResponse>> result = await _service.GetBudgetsByCategoryAsync(999, 1, 50);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }
}
