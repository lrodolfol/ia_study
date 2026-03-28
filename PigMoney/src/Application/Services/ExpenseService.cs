//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Services;

using Application.DTOs.Common;
using Application.DTOs.Expenses;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

public class ExpenseService(
    IExpenseRepository expenseRepository,
    ICategoryRepository categoryRepository,
    IAccountRepository accountRepository,
    ILogger<ExpenseService> logger) : IExpenseService
{
    public async Task<Result<ExpenseResponse>> CreateExpenseAsync(CreateExpenseRequest request)
    {
        logger.LogInformation(
            "Creating expense with amount {Amount} for category {CategoryId}",
            request.Amount,
            request.CategoryId);

        Result<Category> categoryResult = await categoryRepository.GetByIdAsync(request.CategoryId);

        if (!categoryResult.IsSuccess)
        {
            logger.LogWarning("Category with id {CategoryId} not found", request.CategoryId);
            return Result<ExpenseResponse>.Failure("Category not found");
        }

        Result<Account> accountResult = await accountRepository.GetByIdAsync(request.AccountId);

        if (!accountResult.IsSuccess)
        {
            logger.LogWarning("Account with id {AccountId} not found", request.AccountId);
            return Result<ExpenseResponse>.Failure("Account not found");
        }

        Expense expense = new()
        {
            Amount = request.Amount,
            Date = request.Date,
            CategoryId = request.CategoryId,
            AccountId = request.AccountId,
            Description = request.Description ?? string.Empty,
            Notes = request.Notes ?? string.Empty
        };

        Result<Expense> result = await expenseRepository.AddAsync(expense);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create expense: {Error}", result.Error);
            return Result<ExpenseResponse>.Failure(result.Error);
        }

        Result saveResult = await expenseRepository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save expense: {Error}", saveResult.Error);
            return Result<ExpenseResponse>.Failure(saveResult.Error);
        }

        ExpenseResponse response = MapToResponse(
            result.Value!,
            categoryResult.Value!.Name,
            accountResult.Value!.Name);

        logger.LogInformation("Expense created with id {Id}", response.Id);

        return Result<ExpenseResponse>.Success(response);
    }

    public async Task<Result<PaginatedList<ExpenseResponse>>> GetExpensesAsync(ExpenseFilterParams filters)
    {
        logger.LogInformation(
            "Getting expenses with filters - StartDate: {StartDate}, EndDate: {EndDate}, CategoryId: {CategoryId}, AccountId: {AccountId}, Page: {Page}, PageSize: {PageSize}",
            filters.StartDate,
            filters.EndDate,
            filters.CategoryId,
            filters.AccountId,
            filters.Page,
            filters.PageSize);

        int page = filters.Page < 1 ? 1 : filters.Page;
        int pageSize = filters.PageSize < 1 ? 50 : filters.PageSize;

        Result<IEnumerable<Expense>> result = await expenseRepository.GetFilteredAsync(
            filters.StartDate,
            filters.EndDate,
            filters.CategoryId,
            filters.AccountId,
            page,
            pageSize);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to get expenses: {Error}", result.Error);
            return Result<PaginatedList<ExpenseResponse>>.Failure(result.Error);
        }

        Result<int> countResult = await expenseRepository.GetTotalCountAsync(
            filters.StartDate,
            filters.EndDate,
            filters.CategoryId,
            filters.AccountId);

        if (!countResult.IsSuccess)
        {
            logger.LogError("Failed to get expense count: {Error}", countResult.Error);
            return Result<PaginatedList<ExpenseResponse>>.Failure(countResult.Error);
        }

        List<ExpenseResponse> items = result.Value!
            .Select(e => MapToResponse(
                e,
                e.Category?.Name ?? string.Empty,
                e.Account?.Name ?? string.Empty))
            .ToList();

        PaginatedList<ExpenseResponse> paginatedList = new(
            items,
            countResult.Value,
            page,
            pageSize);

        return Result<PaginatedList<ExpenseResponse>>.Success(paginatedList);
    }

    public async Task<Result<ExpenseResponse>> GetExpenseByIdAsync(int id)
    {
        logger.LogInformation("Getting expense with id {Id}", id);

        Result<Expense> result = await expenseRepository.GetByIdAsync(id);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Expense with id {Id} not found", id);
            return Result<ExpenseResponse>.Failure(result.Error);
        }

        Expense expense = result.Value!;

        ExpenseResponse response = MapToResponse(
            expense,
            expense.Category?.Name ?? string.Empty,
            expense.Account?.Name ?? string.Empty);

        return Result<ExpenseResponse>.Success(response);
    }

    public async Task<Result<ExpenseResponse>> UpdateExpenseAsync(int id, UpdateExpenseRequest request)
    {
        logger.LogInformation("Updating expense with id {Id}", id);

        Result<Expense> getResult = await expenseRepository.GetByIdAsync(id);

        if (!getResult.IsSuccess)
        {
            logger.LogWarning("Expense with id {Id} not found", id);
            return Result<ExpenseResponse>.Failure(getResult.Error);
        }

        Expense expense = getResult.Value!;

        if (request.CategoryId.HasValue)
        {
            Result<Category> categoryResult = await categoryRepository.GetByIdAsync(request.CategoryId.Value);

            if (!categoryResult.IsSuccess)
            {
                logger.LogWarning("Category with id {CategoryId} not found", request.CategoryId);
                return Result<ExpenseResponse>.Failure("Category not found");
            }

            expense.CategoryId = request.CategoryId.Value;
            expense.Category = categoryResult.Value;
        }

        if (request.AccountId.HasValue)
        {
            Result<Account> accountResult = await accountRepository.GetByIdAsync(request.AccountId.Value);

            if (!accountResult.IsSuccess)
            {
                logger.LogWarning("Account with id {AccountId} not found", request.AccountId);
                return Result<ExpenseResponse>.Failure("Account not found");
            }

            expense.AccountId = request.AccountId.Value;
            expense.Account = accountResult.Value;
        }

        if (request.Amount.HasValue)
        {
            expense.Amount = request.Amount.Value;
        }

        if (request.Date.HasValue)
        {
            expense.Date = request.Date.Value;
        }

        if (request.Description is not null)
        {
            expense.Description = request.Description;
        }

        if (request.Notes is not null)
        {
            expense.Notes = request.Notes;
        }

        Result<Expense> updateResult = await expenseRepository.UpdateAsync(expense);

        if (!updateResult.IsSuccess)
        {
            logger.LogError("Failed to update expense: {Error}", updateResult.Error);
            return Result<ExpenseResponse>.Failure(updateResult.Error);
        }

        Result saveResult = await expenseRepository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save expense: {Error}", saveResult.Error);
            return Result<ExpenseResponse>.Failure(saveResult.Error);
        }

        ExpenseResponse response = MapToResponse(
            expense,
            expense.Category?.Name ?? string.Empty,
            expense.Account?.Name ?? string.Empty);

        logger.LogInformation("Expense with id {Id} updated", id);

        return Result<ExpenseResponse>.Success(response);
    }

    public async Task<Result> DeleteExpenseAsync(int id)
    {
        logger.LogInformation("Deleting expense with id {Id}", id);

        Result<Expense> getResult = await expenseRepository.GetByIdAsync(id);

        if (!getResult.IsSuccess)
        {
            logger.LogWarning("Expense with id {Id} not found", id);
            return Result.Failure(getResult.Error);
        }

        Result deleteResult = await expenseRepository.DeleteAsync(id);

        if (!deleteResult.IsSuccess)
        {
            logger.LogError("Failed to delete expense: {Error}", deleteResult.Error);
            return Result.Failure(deleteResult.Error);
        }

        Result saveResult = await expenseRepository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save expense deletion: {Error}", saveResult.Error);
            return Result.Failure(saveResult.Error);
        }

        logger.LogInformation("Expense with id {Id} deleted", id);

        return Result.Success();
    }

    private static ExpenseResponse MapToResponse(Expense expense, string categoryName, string accountName)
    {
        return new ExpenseResponse(
            expense.Id,
            expense.Amount,
            expense.Date,
            expense.CategoryId,
            categoryName,
            expense.AccountId,
            accountName,
            expense.Description,
            expense.Notes,
            expense.CreatedAt,
            expense.UpdatedAt);
    }
}
