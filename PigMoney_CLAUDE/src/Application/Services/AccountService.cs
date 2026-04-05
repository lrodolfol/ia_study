//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Accounts;
using Application.DTOs.Common;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class AccountService(IAccountRepository accountRepository, ILogger<AccountService> logger) : IAccountService
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ILogger<AccountService> _logger = logger;

    public async Task<Result<PaginatedList<AccountResponse>>> GetAllAsync(int page, int pageSize)
    {
        var accounts = await _accountRepository.GetPagedAsync(page, pageSize);
        int totalCount = await _accountRepository.CountAsync();

        var items = accounts.Select(MapToResponse);

        return Result<PaginatedList<AccountResponse>>.Success(
            new PaginatedList<AccountResponse>(items, totalCount, page, pageSize));
    }

    public async Task<Result<AccountResponse>> GetByIdAsync(int id)
    {
        Account? account = await _accountRepository.GetByIdAsync(id);
        if (account is null)
            return Result<AccountResponse>.Failure("Account not found.");

        return Result<AccountResponse>.Success(MapToResponse(account));
    }

    public async Task<Result<AccountResponse>> CreateAsync(CreateAccountRequest request)
    {
        var account = new Account
        {
            Name = request.Name,
            Type = request.Type,
            Balance = request.Balance,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Account created = await _accountRepository.AddAsync(account);
        _logger.LogInformation("Created {EntityType} with Id {EntityId}", "Account", created.Id);

        return Result<AccountResponse>.Success(MapToResponse(created));
    }

    public async Task<Result<AccountResponse>> UpdateAsync(int id, UpdateAccountRequest request)
    {
        Account? account = await _accountRepository.GetByIdAsync(id);
        if (account is null)
            return Result<AccountResponse>.Failure("Account not found.");

        account.Name = request.Name;
        account.Type = request.Type;
        account.Balance = request.Balance;
        account.UpdatedAt = DateTime.UtcNow;

        await _accountRepository.UpdateAsync(account);
        _logger.LogInformation("Updated {EntityType} with Id {EntityId}", "Account", account.Id);

        return Result<AccountResponse>.Success(MapToResponse(account));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        Account? account = await _accountRepository.GetByIdAsync(id);
        if (account is null)
            return Result<bool>.Failure("Account not found.");

        if (account.Incomes.Count > 0 || account.Expenses.Count > 0)
        {
            int dependentCount = account.Incomes.Count + account.Expenses.Count;
            _logger.LogWarning("Delete blocked for {EntityType} {EntityId}: {DependentCount} dependents",
                "Account", id, dependentCount);
            return Result<bool>.Failure("Cannot delete account with linked incomes or expenses.");
        }

        await _accountRepository.DeleteAsync(account);
        _logger.LogInformation("Deleted {EntityType} with Id {EntityId}", "Account", id);

        return Result<bool>.Success(true);
    }

    private static AccountResponse MapToResponse(Account account) =>
        new(account.Id, account.Name, account.Type, account.Balance, account.CreatedAt, account.UpdatedAt);
}
