//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Services;

using Application.DTOs.Common;
using Application.DTOs.ExpenseItems;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

public class ExpenseItemService(
    IExpenseItemRepository expenseItemRepository,
    IExpenseRepository expenseRepository,
    ILogger<ExpenseItemService> logger) : IExpenseItemService
{
    public async Task<Result<ExpenseItemResponse>> CreateExpenseItemAsync(CreateExpenseItemRequest request)
    {
        logger.LogInformation(
            "Creating expense item for expense {ExpenseId} with amount {Amount}",
            request.ExpenseId,
            request.Amount);

        Result<Expense> expenseResult = await expenseRepository.GetByIdAsync(request.ExpenseId);

        if (!expenseResult.IsSuccess)
        {
            logger.LogWarning("Expense with id {ExpenseId} not found", request.ExpenseId);
            return Result<ExpenseItemResponse>.Failure("Expense not found");
        }

        Expense parentExpense = expenseResult.Value!;

        Result<decimal> sumResult = await expenseItemRepository.GetSumByExpenseIdAsync(request.ExpenseId);

        if (!sumResult.IsSuccess)
        {
            logger.LogError("Failed to get expense items sum: {Error}", sumResult.Error);
            return Result<ExpenseItemResponse>.Failure(sumResult.Error);
        }

        decimal currentSum = sumResult.Value;

        if (currentSum + request.Amount > parentExpense.Amount)
        {
            logger.LogWarning(
                "Sum of expense items ({CurrentSum} + {NewAmount} = {Total}) exceeds parent expense amount ({ParentAmount})",
                currentSum,
                request.Amount,
                currentSum + request.Amount,
                parentExpense.Amount);
            return Result<ExpenseItemResponse>.Failure("Sum of expense items cannot exceed parent expense amount");
        }

        ExpenseItem expenseItem = new()
        {
            ExpenseId = request.ExpenseId,
            Amount = request.Amount,
            Description = request.Description,
            CategoryId = request.CategoryId
        };

        Result<ExpenseItem> result = await expenseItemRepository.AddAsync(expenseItem);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create expense item: {Error}", result.Error);
            return Result<ExpenseItemResponse>.Failure(result.Error);
        }

        Result saveResult = await expenseItemRepository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save expense item: {Error}", saveResult.Error);
            return Result<ExpenseItemResponse>.Failure(saveResult.Error);
        }

        ExpenseItemResponse response = MapToResponse(result.Value!, null);

        logger.LogInformation("Expense item created with id {Id}", response.Id);

        return Result<ExpenseItemResponse>.Success(response);
    }

    public async Task<Result<PaginatedList<ExpenseItemResponse>>> GetExpenseItemsByExpenseIdAsync(int expenseId, int page, int pageSize)
    {
        logger.LogInformation(
            "Getting expense items for expense {ExpenseId}, page {Page}, size {PageSize}",
            expenseId,
            page,
            pageSize);

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 50;
        }

        Result<IEnumerable<ExpenseItem>> result = await expenseItemRepository.GetByExpenseIdAsync(expenseId, page, pageSize);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to get expense items: {Error}", result.Error);
            return Result<PaginatedList<ExpenseItemResponse>>.Failure(result.Error);
        }

        Result<int> countResult = await expenseItemRepository.GetTotalCountByExpenseAsync(expenseId);

        if (!countResult.IsSuccess)
        {
            logger.LogError("Failed to get expense item count: {Error}", countResult.Error);
            return Result<PaginatedList<ExpenseItemResponse>>.Failure(countResult.Error);
        }

        List<ExpenseItemResponse> items = result.Value!
            .Select(ei => MapToResponse(ei, ei.Category?.Name))
            .ToList();

        PaginatedList<ExpenseItemResponse> paginatedList = new(
            items,
            countResult.Value,
            page,
            pageSize);

        return Result<PaginatedList<ExpenseItemResponse>>.Success(paginatedList);
    }

    public async Task<Result<ExpenseItemResponse>> GetExpenseItemByIdAsync(int id)
    {
        logger.LogInformation("Getting expense item with id {Id}", id);

        Result<ExpenseItem> result = await expenseItemRepository.GetByIdAsync(id);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Expense item with id {Id} not found", id);
            return Result<ExpenseItemResponse>.Failure(result.Error);
        }

        ExpenseItem expenseItem = result.Value!;

        ExpenseItemResponse response = MapToResponse(expenseItem, expenseItem.Category?.Name);

        return Result<ExpenseItemResponse>.Success(response);
    }

    public async Task<Result<ExpenseItemResponse>> UpdateExpenseItemAsync(int id, UpdateExpenseItemRequest request)
    {
        logger.LogInformation("Updating expense item with id {Id}", id);

        Result<ExpenseItem> getResult = await expenseItemRepository.GetByIdAsync(id);

        if (!getResult.IsSuccess)
        {
            logger.LogWarning("Expense item with id {Id} not found", id);
            return Result<ExpenseItemResponse>.Failure(getResult.Error);
        }

        ExpenseItem expenseItem = getResult.Value!;

        if (request.Amount.HasValue)
        {
            Result<Expense> expenseResult = await expenseRepository.GetByIdAsync(expenseItem.ExpenseId);

            if (!expenseResult.IsSuccess)
            {
                logger.LogError("Parent expense not found for expense item {Id}", id);
                return Result<ExpenseItemResponse>.Failure("Parent expense not found");
            }

            Expense parentExpense = expenseResult.Value!;

            Result<decimal> sumResult = await expenseItemRepository.GetSumByExpenseIdAsync(expenseItem.ExpenseId);

            if (!sumResult.IsSuccess)
            {
                logger.LogError("Failed to get expense items sum: {Error}", sumResult.Error);
                return Result<ExpenseItemResponse>.Failure(sumResult.Error);
            }

            decimal currentSum = sumResult.Value;
            decimal newSum = currentSum - expenseItem.Amount + request.Amount.Value;

            if (newSum > parentExpense.Amount)
            {
                logger.LogWarning(
                    "Sum of expense items after update ({NewSum}) exceeds parent expense amount ({ParentAmount})",
                    newSum,
                    parentExpense.Amount);
                return Result<ExpenseItemResponse>.Failure("Sum of expense items cannot exceed parent expense amount");
            }

            expenseItem.Amount = request.Amount.Value;
        }

        if (request.Description is not null)
        {
            expenseItem.Description = request.Description;
        }

        if (request.CategoryId.HasValue)
        {
            expenseItem.CategoryId = request.CategoryId.Value;
        }

        Result<ExpenseItem> updateResult = await expenseItemRepository.UpdateAsync(expenseItem);

        if (!updateResult.IsSuccess)
        {
            logger.LogError("Failed to update expense item: {Error}", updateResult.Error);
            return Result<ExpenseItemResponse>.Failure(updateResult.Error);
        }

        Result saveResult = await expenseItemRepository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save expense item: {Error}", saveResult.Error);
            return Result<ExpenseItemResponse>.Failure(saveResult.Error);
        }

        ExpenseItemResponse response = MapToResponse(expenseItem, expenseItem.Category?.Name);

        logger.LogInformation("Expense item with id {Id} updated", id);

        return Result<ExpenseItemResponse>.Success(response);
    }

    public async Task<Result> DeleteExpenseItemAsync(int id)
    {
        logger.LogInformation("Deleting expense item with id {Id}", id);

        Result<ExpenseItem> getResult = await expenseItemRepository.GetByIdAsync(id);

        if (!getResult.IsSuccess)
        {
            logger.LogWarning("Expense item with id {Id} not found", id);
            return Result.Failure(getResult.Error);
        }

        Result deleteResult = await expenseItemRepository.DeleteAsync(id);

        if (!deleteResult.IsSuccess)
        {
            logger.LogError("Failed to delete expense item: {Error}", deleteResult.Error);
            return Result.Failure(deleteResult.Error);
        }

        Result saveResult = await expenseItemRepository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save expense item deletion: {Error}", saveResult.Error);
            return Result.Failure(saveResult.Error);
        }

        logger.LogInformation("Expense item with id {Id} deleted", id);

        return Result.Success();
    }

    private static ExpenseItemResponse MapToResponse(ExpenseItem expenseItem, string? categoryName)
    {
        return new ExpenseItemResponse(
            expenseItem.Id,
            expenseItem.ExpenseId,
            expenseItem.Amount,
            expenseItem.Description,
            expenseItem.CategoryId,
            categoryName,
            expenseItem.CreatedAt,
            expenseItem.UpdatedAt);
    }
}
