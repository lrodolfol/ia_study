//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Accounts;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace pigMoney.Tests.Application.Services;

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _repositoryMock = new();
    private readonly Mock<ILogger<AccountService>> _loggerMock = new();
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _service = new AccountService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess()
    {
        var request = new CreateAccountRequest("Checking Account", AccountType.Checking, 1000m);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account a) => { a.Id = 1; return a; });

        var result = await _service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("Checking Account", result.Value!.Name);
        Assert.Equal(AccountType.Checking, result.Value.Type);
        Assert.Equal(1000m, result.Value.Balance);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Account?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found.", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ShouldReturnSuccess()
    {
        var account = new Account { Id = 1, Name = "My Account", Type = AccountType.Savings, Balance = 500m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);

        var result = await _service.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Account?)null);

        var result = await _service.UpdateAsync(99, new UpdateAccountRequest("Updated", AccountType.Cash, 0));

        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_ShouldReturnSuccess()
    {
        var account = new Account { Id = 1, Name = "Old", Type = AccountType.Checking, Balance = 100m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);

        var result = await _service.UpdateAsync(1, new UpdateAccountRequest("New Name", AccountType.Savings, 200m));

        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", result.Value!.Name);
        Assert.Equal(AccountType.Savings, result.Value.Type);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ShouldReturnFailure()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Account?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenHasDependents_ShouldReturn409Failure()
    {
        var account = new Account
        {
            Id = 1,
            Name = "My Account",
            Incomes = [new Income { Id = 1 }],
            Expenses = []
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);

        var result = await _service.DeleteAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot delete account", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenNoDependents_ShouldReturnSuccess()
    {
        var account = new Account { Id = 1, Name = "My Account", Incomes = [], Expenses = [] };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);

        var result = await _service.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.DeleteAsync(account), Times.Once);
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

    [Fact]
    public async Task GetAllAsync_WithData_ShouldReturnPaginatedList()
    {
        var accounts = new List<Account>
        {
            new() { Id = 1, Name = "Checking", Type = AccountType.Checking, Balance = 1000m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Savings", Type = AccountType.Savings, Balance = 5000m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _repositoryMock.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync(accounts);
        _repositoryMock.Setup(r => r.CountAsync()).ReturnsAsync(2);

        var result = await _service.GetAllAsync(1, 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Items.Count());
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(10, result.Value.PageSize);
    }
}
