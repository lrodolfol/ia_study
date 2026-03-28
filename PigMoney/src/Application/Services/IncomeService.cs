//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Services;

using Application.DTOs.Common;
using Application.DTOs.Incomes;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

public class IncomeService(
    IIncomeRepository incomeRepository,
    ICategoryRepository categoryRepository,
    IAccountRepository accountRepository,
    ILogger<IncomeService> logger) : IIncomeService
{
    public async Task<Result<IncomeResponse>> CreateIncomeAsync(CreateIncomeRequest request)
    {
        logger.LogInformation(
            "Creating income with amount {Amount} for category {CategoryId}",
            request.Amount,
            request.CategoryId);

        Result<Category> categoryResult = await categoryRepository.GetByIdAsync(request.CategoryId);

        if (!categoryResult.IsSuccess)
        {
            logger.LogWarning("Category with id {CategoryId} not found", request.CategoryId);
            return Result<IncomeResponse>.Failure("Category not found");
        }

        Result<Account> accountResult = await accountRepository.GetByIdAsync(request.AccountId);

        if (!accountResult.IsSuccess)
        {
            logger.LogWarning("Account with id {AccountId} not found", request.AccountId);
            return Result<IncomeResponse>.Failure("Account not found");
        }

        Income income = new()
        {
            Amount = request.Amount,
            Date = request.Date,
            CategoryId = request.CategoryId,
            AccountId = request.AccountId,
            Description = request.Description ?? string.Empty,
            Notes = request.Notes ?? string.Empty
        };

        Result<Income> result = await incomeRepository.AddAsync(income);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create income: {Error}", result.Error);
            return Result<IncomeResponse>.Failure(result.Error);
        }

        Result saveResult = await incomeRepository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save income: {Error}", saveResult.Error);
            return Result<IncomeResponse>.Failure(saveResult.Error);
        }

        IncomeResponse response = MapToResponse(
            result.Value!,
            categoryResult.Value!.Name,
            accountResult.Value!.Name);

        logger.LogInformation("Income created with id {Id}", response.Id);

        return Result<IncomeResponse>.Success(response);
    }

    public async Task<Result<PaginatedList<IncomeResponse>>> GetIncomesAsync(IncomeFilterParams filters)
    {
        logger.LogInformation(
            "Getting incomes with filters - StartDate: {StartDate}, EndDate: {EndDate}, CategoryId: {CategoryId}, AccountId: {AccountId}, Page: {Page}, PageSize: {PageSize}",
            filters.StartDate,
            filters.EndDate,
            filters.CategoryId,
            filters.AccountId,
            filters.Page,
            filters.PageSize);

        int page = filters.Page < 1 ? 1 : filters.Page;
        int pageSize = filters.PageSize < 1 ? 50 : filters.PageSize;

        Result<IEnumerable<Income>> result = await incomeRepository.GetFilteredAsync(
            filters.StartDate,
            filters.EndDate,
            filters.CategoryId,
            filters.AccountId,
            page,
            pageSize);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to get incomes: {Error}", result.Error);
            return Result<PaginatedList<IncomeResponse>>.Failure(result.Error);
        }

        Result<int> countResult = await incomeRepository.GetTotalCountAsync(
            filters.StartDate,
            filters.EndDate,
            filters.CategoryId,
            filters.AccountId);

        if (!countResult.IsSuccess)
        {
            logger.LogError("Failed to get income count: {Error}", countResult.Error);
            return Result<PaginatedList<IncomeResponse>>.Failure(countResult.Error);
        }

        List<IncomeResponse> items = result.Value!
            .Select(e => MapToResponse(
                e,
                e.Category?.Name ?? string.Empty,
                e.Account?.Name ?? string.Empty))
            .ToList();

        PaginatedList<IncomeResponse> paginatedList = new(
            items,
            countResult.Value,
            page,
            pageSize);

        return Result<PaginatedList<IncomeResponse>>.Success(paginatedList);
    }

    public async Task<Result<IncomeResponse>> GetIncomeByIdAsync(int id)
    {
        logger.LogInformation("Getting income with id {Id}", id);

        Result<Income> result = await incomeRepository.GetByIdAsync(id);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Income with id {Id} not found", id);
            return Result<IncomeResponse>.Failure(result.Error);
        }

        Income income = result.Value!;

        IncomeResponse response = MapToResponse(
            income,
            income.Category?.Name ?? string.Empty,
            income.Account?.Name ?? string.Empty);

        return Result<IncomeResponse>.Success(response);
    }

    public async Task<Result<IncomeResponse>> UpdateIncomeAsync(int id, UpdateIncomeRequest request)
    {
        logger.LogInformation("Updating income with id {Id}", id);

        Result<Income> getResult = await incomeRepository.GetByIdAsync(id);

        if (!getResult.IsSuccess)
        {
            logger.LogWarning("Income with id {Id} not found", id);
            return Result<IncomeResponse>.Failure(getResult.Error);
        }

        Income income = getResult.Value!;

        if (request.CategoryId.HasValue)
        {
            Result<Category> categoryResult = await categoryRepository.GetByIdAsync(request.CategoryId.Value);

            if (!categoryResult.IsSuccess)
            {
                logger.LogWarning("Category with id {CategoryId} not found", request.CategoryId);
                return Result<IncomeResponse>.Failure("Category not found");
            }

            income.CategoryId = request.CategoryId.Value;
            income.Category = categoryResult.Value;
        }

        if (request.AccountId.HasValue)
        {
            Result<Account> accountResult = await accountRepository.GetByIdAsync(request.AccountId.Value);

            if (!accountResult.IsSuccess)
            {
                logger.LogWarning("Account with id {AccountId} not found", request.AccountId);
                return Result<IncomeResponse>.Failure("Account not found");
            }

            income.AccountId = request.AccountId.Value;
            income.Account = accountResult.Value;
        }

        if (request.Amount.HasValue)
        {
            income.Amount = request.Amount.Value;
        }

        if (request.Date.HasValue)
        {
            income.Date = request.Date.Value;
        }

        if (request.Description is not null)
        {
            income.Description = request.Description;
        }

        if (request.Notes is not null)
        {
            income.Notes = request.Notes;
        }

        Result<Income> updateResult = await incomeRepository.UpdateAsync(income);

        if (!updateResult.IsSuccess)
        {
            logger.LogError("Failed to update income: {Error}", updateResult.Error);
            return Result<IncomeResponse>.Failure(updateResult.Error);
        }

        Result saveResult = await incomeRepository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save income: {Error}", saveResult.Error);
            return Result<IncomeResponse>.Failure(saveResult.Error);
        }

        IncomeResponse response = MapToResponse(
            income,
            income.Category?.Name ?? string.Empty,
            income.Account?.Name ?? string.Empty);

        logger.LogInformation("Income with id {Id} updated", id);

        return Result<IncomeResponse>.Success(response);
    }

    public async Task<Result> DeleteIncomeAsync(int id)
    {
        logger.LogInformation("Deleting income with id {Id}", id);

        Result<Income> getResult = await incomeRepository.GetByIdAsync(id);

        if (!getResult.IsSuccess)
        {
            logger.LogWarning("Income with id {Id} not found", id);
            return Result.Failure(getResult.Error);
        }

        Result deleteResult = await incomeRepository.DeleteAsync(id);

        if (!deleteResult.IsSuccess)
        {
            logger.LogError("Failed to delete income: {Error}", deleteResult.Error);
            return Result.Failure(deleteResult.Error);
        }

        Result saveResult = await incomeRepository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save income deletion: {Error}", saveResult.Error);
            return Result.Failure(saveResult.Error);
        }

        logger.LogInformation("Income with id {Id} deleted", id);

        return Result.Success();
    }

    private static IncomeResponse MapToResponse(Income income, string categoryName, string accountName)
    {
        return new IncomeResponse(
            income.Id,
            income.Amount,
            income.Date,
            income.CategoryId,
            categoryName,
            income.AccountId,
            accountName,
            income.Description,
            income.Notes,
            income.CreatedAt,
            income.UpdatedAt);
    }
}
