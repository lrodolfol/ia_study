//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Categories;
using Application.DTOs.Common;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly Mock<ILogger<CategoryService>> _loggerMock;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _loggerMock = new Mock<ILogger<CategoryService>>();
        _service = new CategoryService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateCategoryAsync_ReturnsSuccess_WithCategoryResponse()
    {
        CreateCategoryRequest request = new("Food", TransactionType.Expense);

        Category category = new()
        {
            Id = 1,
            Name = "Food",
            Type = TransactionType.Expense,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Category>()))
            .ReturnsAsync(Result<Category>.Success(category));

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        Result<CategoryResponse> result = await _service.CreateCategoryAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Food", result.Value.Name);
        Assert.Equal(TransactionType.Expense, result.Value.Type);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ReturnsFailure_WhenNotFound()
    {
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(Result<Category>.Failure("Category not found"));

        Result<CategoryResponse> result = await _service.GetCategoryByIdAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ReturnsFailure_WhenHasActiveExpenses()
    {
        Category category = new()
        {
            Id = 1,
            Name = "Food",
            Type = TransactionType.Expense,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Category>.Success(category));

        _repositoryMock
            .Setup(x => x.HasDependenciesAsync(1))
            .ReturnsAsync(Result<bool>.Success(true));

        Result result = await _service.DeleteCategoryAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Contains("active transactions", result.Error);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ReturnsSuccess_WhenNoActiveTransactions()
    {
        Category category = new()
        {
            Id = 1,
            Name = "Food",
            Type = TransactionType.Expense,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Category>.Success(category));

        _repositoryMock
            .Setup(x => x.HasDependenciesAsync(1))
            .ReturnsAsync(Result<bool>.Success(false));

        _repositoryMock
            .Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(Result.Success());

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        Result result = await _service.DeleteCategoryAsync(1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ReturnsFailure_WhenCategoryNotFound()
    {
        UpdateCategoryRequest request = new("Updated Name", null);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync(Result<Category>.Failure("Category not found"));

        Result<CategoryResponse> result = await _service.UpdateCategoryAsync(999, request);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsPaginatedResults()
    {
        List<Category> categories =
        [
            new() { Id = 1, Name = "Food", Type = TransactionType.Expense, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Salary", Type = TransactionType.Income, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        ];

        _repositoryMock
            .Setup(x => x.GetAllAsync(1, 50))
            .ReturnsAsync(Result<IEnumerable<Category>>.Success(categories));

        _repositoryMock
            .Setup(x => x.GetTotalCountAsync())
            .ReturnsAsync(Result<int>.Success(2));

        Result<PaginatedList<CategoryResponse>> result = await _service.GetAllCategoriesAsync(1, 50);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(1, result.Value.TotalPages);
    }

    [Fact]
    public async Task CorrectlyMapsEntityToResponse()
    {
        Category category = new()
        {
            Id = 1,
            Name = "Food",
            Type = TransactionType.Expense,
            CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 1, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Category>.Success(category));

        Result<CategoryResponse> result = await _service.GetCategoryByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Food", result.Value.Name);
        Assert.Equal(TransactionType.Expense, result.Value.Type);
        Assert.Equal(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc), result.Value.CreatedAt);
        Assert.Equal(new DateTime(2025, 1, 2, 12, 0, 0, DateTimeKind.Utc), result.Value.UpdatedAt);
    }
}
