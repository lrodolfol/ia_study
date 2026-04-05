//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Incomes;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace pigMoney.Tests.Application.Services;

public class IncomeServiceTests
{
    private readonly Mock<IIncomeRepository> _incomeRepositoryMock = new();
    private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
    private readonly Mock<ILogger<IncomeService>> _loggerMock = new();
    private readonly IncomeService _service;

    public IncomeServiceTests()
    {
        _service = new IncomeService(_incomeRepositoryMock.Object, _accountRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess()
    {
        var request = new CreateIncomeRequest(1500m, DateTime.UtcNow, "Salary", 1);
        _accountRepositoryMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _incomeRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Income>()))
            .ReturnsAsync((Income i) => { i.Id = 1; return i; });

        var result = await _service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1500m, result.Value!.Amount);
        Assert.Equal("Salary", result.Value.Description);
        Assert.Equal(1, result.Value.AccountId);
    }

    [Fact]
    public async Task CreateAsync_WhenAccountNotFound_ShouldReturnFailure()
    {
        var request = new CreateIncomeRequest(1500m, DateTime.UtcNow, "Salary", 99);
        _accountRepositoryMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        var result = await _service.CreateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_ShouldReturnSuccess()
    {
        var income = new Income { Id = 1, Amount = 1000m, Date = DateTime.UtcNow, Description = "Old", AccountId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _incomeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(income);
        _accountRepositoryMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        var result = await _service.UpdateAsync(1, new UpdateIncomeRequest(2000m, DateTime.UtcNow, "Updated", 1));

        Assert.True(result.IsSuccess);
        Assert.Equal(2000m, result.Value!.Amount);
        Assert.Equal("Updated", result.Value.Description);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldReturnFailure()
    {
        _incomeRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Income?)null);

        var result = await _service.UpdateAsync(99, new UpdateIncomeRequest(2000m, DateTime.UtcNow, "Updated", 1));

        Assert.False(result.IsSuccess);
        Assert.Equal("Income not found.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenAccountNotFound_ShouldReturnFailure()
    {
        var income = new Income { Id = 1, Amount = 1000m, Date = DateTime.UtcNow, Description = "Test", AccountId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _incomeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(income);
        _accountRepositoryMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        var result = await _service.UpdateAsync(1, new UpdateIncomeRequest(2000m, DateTime.UtcNow, "Updated", 99));

        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_ShouldReturnSuccess()
    {
        var income = new Income { Id = 1, Amount = 1000m, Date = DateTime.UtcNow, Description = "Test", AccountId = 1 };
        _incomeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(income);

        var result = await _service.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _incomeRepositoryMock.Verify(r => r.DeleteAsync(income), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ShouldReturnFailure()
    {
        _incomeRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Income?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("Income not found.", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnFailure()
    {
        _incomeRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Income?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("Income not found.", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ShouldReturnSuccess()
    {
        var income = new Income { Id = 1, Amount = 500m, Date = DateTime.UtcNow, Description = "Bonus", AccountId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _incomeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(income);

        var result = await _service.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Id);
        Assert.Equal(500m, result.Value.Amount);
    }

    [Fact]
    public async Task GetAllAsync_WithFilters_ShouldReturnFilteredResults()
    {
        var filters = new IncomeFilterParams(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, 1);
        _incomeRepositoryMock.Setup(r => r.GetFilteredAsync(It.IsAny<Domain.Common.IncomeFilterParams>(), 1, 10))
            .ReturnsAsync([]);
        _incomeRepositoryMock.Setup(r => r.CountFilteredAsync(It.IsAny<Domain.Common.IncomeFilterParams>()))
            .ReturnsAsync(0);

        var result = await _service.GetAllAsync(filters, 1, 10);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_NoFilters_ShouldReturnAllPaginated()
    {
        var filters = new IncomeFilterParams(null, null, null);
        var incomes = new List<Income>
        {
            new() { Id = 1, Amount = 100m, Date = DateTime.UtcNow, Description = "Income 1", AccountId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _incomeRepositoryMock.Setup(r => r.GetFilteredAsync(It.IsAny<Domain.Common.IncomeFilterParams>(), 1, 10))
            .ReturnsAsync(incomes);
        _incomeRepositoryMock.Setup(r => r.CountFilteredAsync(It.IsAny<Domain.Common.IncomeFilterParams>()))
            .ReturnsAsync(1);

        var result = await _service.GetAllAsync(filters, 1, 10);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.TotalCount);
    }
}
