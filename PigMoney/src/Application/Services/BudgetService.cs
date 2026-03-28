//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Services;

using Application.DTOs.Budgets;
using Application.DTOs.Common;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

public class BudgetService(
    IBudgetRepository budgetRepository,
    ICategoryRepository categoryRepository,
    ILogger<BudgetService> logger) : IBudgetService
{
    public async Task<Result<BudgetResponse>> CreateBudgetAsync(CreateBudgetRequest request)
    {
        logger.LogInformation(
            "Creating budget for category {CategoryId} with limit {LimitAmount}",
            request.CategoryId,
            request.LimitAmount);

        Result<Category> categoryResult = await categoryRepository.GetByIdAsync(request.CategoryId);

        if (!categoryResult.IsSuccess)
        {
            logger.LogWarning("Category with id {CategoryId} not found", request.CategoryId);
            return Result<BudgetResponse>.Failure("Category not found");
        }

        Budget budget = new()
        {
            CategoryId = request.CategoryId,
            LimitAmount = request.LimitAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        Result<Budget> result = await budgetRepository.AddAsync(budget);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create budget: {Error}", result.Error);
            return Result<BudgetResponse>.Failure(result.Error);
        }

        Result saveResult = await budgetRepository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save budget: {Error}", saveResult.Error);
            return Result<BudgetResponse>.Failure(saveResult.Error);
        }

        BudgetResponse response = MapToResponse(result.Value!, categoryResult.Value!.Name);

        logger.LogInformation("Budget created with id {Id}", response.Id);

        return Result<BudgetResponse>.Success(response);
    }

    public async Task<Result<PaginatedList<BudgetResponse>>> GetAllBudgetsAsync(int page, int pageSize)
    {
        logger.LogInformation("Getting budgets page {Page} with size {PageSize}", page, pageSize);

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 50;
        }

        Result<IEnumerable<Budget>> result = await budgetRepository.GetAllAsync(page, pageSize);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to get budgets: {Error}", result.Error);
            return Result<PaginatedList<BudgetResponse>>.Failure(result.Error);
        }

        Result<int> countResult = await budgetRepository.GetTotalCountAsync();

        if (!countResult.IsSuccess)
        {
            logger.LogError("Failed to get budget count: {Error}", countResult.Error);
            return Result<PaginatedList<BudgetResponse>>.Failure(countResult.Error);
        }

        List<BudgetResponse> items = result.Value!
            .Select(b => MapToResponse(b, b.Category?.Name ?? string.Empty))
            .ToList();

        PaginatedList<BudgetResponse> paginatedList = new(
            items,
            countResult.Value,
            page,
            pageSize);

        return Result<PaginatedList<BudgetResponse>>.Success(paginatedList);
    }

    public async Task<Result<BudgetResponse>> GetBudgetByIdAsync(int id)
    {
        logger.LogInformation("Getting budget with id {Id}", id);

        Result<Budget> result = await budgetRepository.GetByIdAsync(id);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Budget with id {Id} not found", id);
            return Result<BudgetResponse>.Failure(result.Error);
        }

        Budget budget = result.Value!;

        BudgetResponse response = MapToResponse(budget, budget.Category?.Name ?? string.Empty);

        return Result<BudgetResponse>.Success(response);
    }

    public async Task<Result<PaginatedList<BudgetResponse>>> GetBudgetsByCategoryAsync(int categoryId, int page, int pageSize)
    {
        logger.LogInformation(
            "Getting budgets for category {CategoryId}, page {Page}, size {PageSize}",
            categoryId,
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

        Result<IEnumerable<Budget>> result = await budgetRepository.GetByCategoryIdAsync(categoryId, page, pageSize);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to get budgets: {Error}", result.Error);
            return Result<PaginatedList<BudgetResponse>>.Failure(result.Error);
        }

        Result<int> countResult = await budgetRepository.GetTotalCountByCategoryAsync(categoryId);

        if (!countResult.IsSuccess)
        {
            logger.LogError("Failed to get budget count: {Error}", countResult.Error);
            return Result<PaginatedList<BudgetResponse>>.Failure(countResult.Error);
        }

        List<BudgetResponse> items = result.Value!
            .Select(b => MapToResponse(b, b.Category?.Name ?? string.Empty))
            .ToList();

        PaginatedList<BudgetResponse> paginatedList = new(
            items,
            countResult.Value,
            page,
            pageSize);

        return Result<PaginatedList<BudgetResponse>>.Success(paginatedList);
    }

    public async Task<Result<BudgetResponse>> UpdateBudgetAsync(int id, UpdateBudgetRequest request)
    {
        logger.LogInformation("Updating budget with id {Id}", id);

        Result<Budget> getResult = await budgetRepository.GetByIdAsync(id);

        if (!getResult.IsSuccess)
        {
            logger.LogWarning("Budget with id {Id} not found", id);
            return Result<BudgetResponse>.Failure(getResult.Error);
        }

        Budget budget = getResult.Value!;

        if (request.CategoryId.HasValue)
        {
            Result<Category> categoryResult = await categoryRepository.GetByIdAsync(request.CategoryId.Value);

            if (!categoryResult.IsSuccess)
            {
                logger.LogWarning("Category with id {CategoryId} not found", request.CategoryId);
                return Result<BudgetResponse>.Failure("Category not found");
            }

            budget.CategoryId = request.CategoryId.Value;
            budget.Category = categoryResult.Value;
        }

        if (request.LimitAmount.HasValue)
        {
            budget.LimitAmount = request.LimitAmount.Value;
        }

        if (request.StartDate.HasValue)
        {
            budget.StartDate = request.StartDate.Value;
        }

        if (request.EndDate.HasValue)
        {
            budget.EndDate = request.EndDate.Value;
        }

        Result<Budget> updateResult = await budgetRepository.UpdateAsync(budget);

        if (!updateResult.IsSuccess)
        {
            logger.LogError("Failed to update budget: {Error}", updateResult.Error);
            return Result<BudgetResponse>.Failure(updateResult.Error);
        }

        Result saveResult = await budgetRepository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save budget: {Error}", saveResult.Error);
            return Result<BudgetResponse>.Failure(saveResult.Error);
        }

        BudgetResponse response = MapToResponse(budget, budget.Category?.Name ?? string.Empty);

        logger.LogInformation("Budget with id {Id} updated", id);

        return Result<BudgetResponse>.Success(response);
    }

    public async Task<Result> DeleteBudgetAsync(int id)
    {
        logger.LogInformation("Deleting budget with id {Id}", id);

        Result<Budget> getResult = await budgetRepository.GetByIdAsync(id);

        if (!getResult.IsSuccess)
        {
            logger.LogWarning("Budget with id {Id} not found", id);
            return Result.Failure(getResult.Error);
        }

        Result deleteResult = await budgetRepository.DeleteAsync(id);

        if (!deleteResult.IsSuccess)
        {
            logger.LogError("Failed to delete budget: {Error}", deleteResult.Error);
            return Result.Failure(deleteResult.Error);
        }

        Result saveResult = await budgetRepository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save budget deletion: {Error}", saveResult.Error);
            return Result.Failure(saveResult.Error);
        }

        logger.LogInformation("Budget with id {Id} deleted", id);

        return Result.Success();
    }

    private static BudgetResponse MapToResponse(Budget budget, string categoryName)
    {
        return new BudgetResponse(
            budget.Id,
            budget.CategoryId,
            categoryName,
            budget.LimitAmount,
            budget.StartDate,
            budget.EndDate,
            budget.CreatedAt,
            budget.UpdatedAt);
    }
}
