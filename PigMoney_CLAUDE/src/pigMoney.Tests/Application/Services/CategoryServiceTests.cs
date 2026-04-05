//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Categories;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace pigMoney.Tests.Application.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock = new();
    private readonly Mock<ILogger<CategoryService>> _loggerMock = new();
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _service = new CategoryService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess()
    {
        var request = new CreateCategoryRequest("Food", "Groceries");
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) => { c.Id = 1; return c; });

        var result = await _service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("Food", result.Value!.Name);
        Assert.Equal("Groceries", result.Value.Description);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found.", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ShouldReturnSuccess()
    {
        var category = new Category { Id = 1, Name = "Food", Description = "Groceries", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        var result = await _service.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        var result = await _service.UpdateAsync(99, new UpdateCategoryRequest("Updated", "Desc"));

        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_ShouldReturnSuccess()
    {
        var category = new Category { Id = 1, Name = "Food", Description = "Old", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        var result = await _service.UpdateAsync(1, new UpdateCategoryRequest("Updated", "New Desc"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", result.Value!.Name);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenHasDependents_ShouldReturn409Failure()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Food",
            Expenses = [new Expense { Id = 1 }],
            Budgets = []
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        var result = await _service.DeleteAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot delete category", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenNoDependents_ShouldReturnSuccess()
    {
        var category = new Category { Id = 1, Name = "Food", Expenses = [], Budgets = [] };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        var result = await _service.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.DeleteAsync(category), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyPaginatedList()
    {
        _repositoryMock.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync([]);
        _repositoryMock.Setup(r => r.CountAsync()).ReturnsAsync(0);

        var result = await _service.GetAllAsync(1, 10);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }
}
