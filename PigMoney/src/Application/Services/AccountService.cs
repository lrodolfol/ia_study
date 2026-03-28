//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Services;

using Application.DTOs.Accounts;
using Application.DTOs.Common;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

public class AccountService(IAccountRepository repository, ILogger<AccountService> logger) : IAccountService
{
    public async Task<Result<AccountResponse>> CreateAccountAsync(CreateAccountRequest request)
    {
        logger.LogInformation("Creating account with name {Name}", request.Name);

        Account account = new()
        {
            Name = request.Name,
            Type = request.Type,
            InitialBalance = request.InitialBalance
        };

        Result<Account> result = await repository.AddAsync(account);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create account: {Error}", result.Error);
            return Result<AccountResponse>.Failure(result.Error);
        }

        Result saveResult = await repository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save account: {Error}", saveResult.Error);
            return Result<AccountResponse>.Failure(saveResult.Error);
        }

        AccountResponse response = MapToResponse(result.Value!);

        logger.LogInformation("Account created with id {Id}", response.Id);

        return Result<AccountResponse>.Success(response);
    }

    public async Task<Result<PaginatedList<AccountResponse>>> GetAllAccountsAsync(int page, int pageSize)
    {
        logger.LogInformation("Getting accounts page {Page} with size {PageSize}", page, pageSize);

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 50;
        }

        Result<IEnumerable<Account>> result = await repository.GetAllAsync(page, pageSize);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to get accounts: {Error}", result.Error);
            return Result<PaginatedList<AccountResponse>>.Failure(result.Error);
        }

        Result<int> countResult = await repository.GetTotalCountAsync();

        if (!countResult.IsSuccess)
        {
            logger.LogError("Failed to get account count: {Error}", countResult.Error);
            return Result<PaginatedList<AccountResponse>>.Failure(countResult.Error);
        }

        List<AccountResponse> items = result.Value!.Select(MapToResponse).ToList();

        PaginatedList<AccountResponse> paginatedList = new(
            items,
            countResult.Value,
            page,
            pageSize);

        return Result<PaginatedList<AccountResponse>>.Success(paginatedList);
    }

    public async Task<Result<AccountResponse>> GetAccountByIdAsync(int id)
    {
        logger.LogInformation("Getting account with id {Id}", id);

        Result<Account> result = await repository.GetByIdAsync(id);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Account with id {Id} not found", id);
            return Result<AccountResponse>.Failure(result.Error);
        }

        AccountResponse response = MapToResponse(result.Value!);

        return Result<AccountResponse>.Success(response);
    }

    public async Task<Result<AccountResponse>> UpdateAccountAsync(int id, UpdateAccountRequest request)
    {
        logger.LogInformation("Updating account with id {Id}", id);

        Result<Account> getResult = await repository.GetByIdAsync(id);

        if (!getResult.IsSuccess)
        {
            logger.LogWarning("Account with id {Id} not found", id);
            return Result<AccountResponse>.Failure(getResult.Error);
        }

        Account account = getResult.Value!;

        if (request.Name is not null)
        {
            account.Name = request.Name;
        }

        if (request.Type.HasValue)
        {
            account.Type = request.Type.Value;
        }

        if (request.InitialBalance.HasValue)
        {
            account.InitialBalance = request.InitialBalance.Value;
        }

        Result<Account> updateResult = await repository.UpdateAsync(account);

        if (!updateResult.IsSuccess)
        {
            logger.LogError("Failed to update account: {Error}", updateResult.Error);
            return Result<AccountResponse>.Failure(updateResult.Error);
        }

        Result saveResult = await repository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save account: {Error}", saveResult.Error);
            return Result<AccountResponse>.Failure(saveResult.Error);
        }

        AccountResponse response = MapToResponse(updateResult.Value!);

        logger.LogInformation("Account with id {Id} updated", id);

        return Result<AccountResponse>.Success(response);
    }

    public async Task<Result> DeleteAccountAsync(int id)
    {
        logger.LogInformation("Deleting account with id {Id}", id);

        Result<Account> getResult = await repository.GetByIdAsync(id);

        if (!getResult.IsSuccess)
        {
            logger.LogWarning("Account with id {Id} not found", id);
            return Result.Failure(getResult.Error);
        }

        Result<bool> hasDependenciesResult = await repository.HasDependenciesAsync(id);

        if (!hasDependenciesResult.IsSuccess)
        {
            logger.LogError("Failed to check account dependencies: {Error}", hasDependenciesResult.Error);
            return Result.Failure(hasDependenciesResult.Error);
        }

        if (hasDependenciesResult.Value)
        {
            logger.LogWarning("Cannot delete account with active transactions");
            return Result.Failure("Cannot delete account with active transactions");
        }

        Result deleteResult = await repository.DeleteAsync(id);

        if (!deleteResult.IsSuccess)
        {
            logger.LogError("Failed to delete account: {Error}", deleteResult.Error);
            return Result.Failure(deleteResult.Error);
        }

        Result saveResult = await repository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save account deletion: {Error}", saveResult.Error);
            return Result.Failure(saveResult.Error);
        }

        logger.LogInformation("Account with id {Id} deleted", id);

        return Result.Success();
    }

    private static AccountResponse MapToResponse(Account account)
    {
        return new AccountResponse(
            account.Id,
            account.Name,
            account.Type,
            account.InitialBalance,
            account.CreatedAt,
            account.UpdatedAt);
    }
}
