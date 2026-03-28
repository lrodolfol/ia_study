//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Accounts;
using Application.DTOs.Common;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _repositoryMock;
    private readonly Mock<ILogger<AccountService>> _loggerMock;
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _repositoryMock = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILogger<AccountService>>();
        _service = new AccountService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAccountAsync_ReturnsSuccess_WithAccountResponse()
    {
        CreateAccountRequest request = new("Bank Account", AccountType.Checking, 1000m);

        Account account = new()
        {
            Id = 1,
            Name = "Bank Account",
            Type = AccountType.Checking,
            InitialBalance = 1000m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Account>()))
            .ReturnsAsync(Result<Account>.Success(account));

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        Result<AccountResponse> result = await _service.CreateAccountAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Bank Account", result.Value.Name);
        Assert.Equal(AccountType.Checking, result.Value.Type);
        Assert.Equal(1000m, result.Value.InitialBalance);
    }

    [Fact]
    public async Task GetAccountByIdAsync_ReturnsFailure_WhenNotFound()
    {
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(Result<Account>.Failure("Account not found"));

        Result<AccountResponse> result = await _service.GetAccountByIdAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task DeleteAccountAsync_ReturnsFailure_WhenHasActiveExpenses()
    {
        Account account = new()
        {
            Id = 1,
            Name = "Bank Account",
            Type = AccountType.Checking,
            InitialBalance = 1000m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Account>.Success(account));

        _repositoryMock
            .Setup(x => x.HasDependenciesAsync(1))
            .ReturnsAsync(Result<bool>.Success(true));

        Result result = await _service.DeleteAccountAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Contains("active transactions", result.Error);
    }

    [Fact]
    public async Task DeleteAccountAsync_ReturnsSuccess_WhenNoActiveTransactions()
    {
        Account account = new()
        {
            Id = 1,
            Name = "Bank Account",
            Type = AccountType.Checking,
            InitialBalance = 1000m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Account>.Success(account));

        _repositoryMock
            .Setup(x => x.HasDependenciesAsync(1))
            .ReturnsAsync(Result<bool>.Success(false));

        _repositoryMock
            .Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(Result.Success());

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        Result result = await _service.DeleteAccountAsync(1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CorrectlyMapsEntityToResponseWithInitialBalance()
    {
        Account account = new()
        {
            Id = 1,
            Name = "Savings",
            Type = AccountType.Savings,
            InitialBalance = 5000.50m,
            CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 1, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result<Account>.Success(account));

        Result<AccountResponse> result = await _service.GetAccountByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Savings", result.Value.Name);
        Assert.Equal(AccountType.Savings, result.Value.Type);
        Assert.Equal(5000.50m, result.Value.InitialBalance);
        Assert.Equal(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc), result.Value.CreatedAt);
        Assert.Equal(new DateTime(2025, 1, 2, 12, 0, 0, DateTimeKind.Utc), result.Value.UpdatedAt);
    }
}
