//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Budgets;
using Application.DTOs.Common;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class BudgetService(IBudgetRepository budgetRepository, ICategoryRepository categoryRepository, ILogger<BudgetService> logger) : IBudgetService
{
    private readonly IBudgetRepository _budgetRepository = budgetRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ILogger<BudgetService> _logger = logger;

    public async Task<Result<PaginatedList<BudgetResponse>>> GetAllAsync(BudgetFilterParams filters, int page, int pageSize)
    {
        var budgets = await _budgetRepository.GetFilteredAsync(filters, page, pageSize);
        int totalCount = await _budgetRepository.CountFilteredAsync(filters);

        var items = budgets.Select(MapToResponse);

        return Result<PaginatedList<BudgetResponse>>.Success(
            new PaginatedList<BudgetResponse>(items, totalCount, page, pageSize));
    }

    public async Task<Result<BudgetResponse>> GetByIdAsync(int id)
    {
        Budget? budget = await _budgetRepository.GetByIdAsync(id);
        if (budget is null)
            return Result<BudgetResponse>.Failure("Budget not found.");

        return Result<BudgetResponse>.Success(MapToResponse(budget));
    }

    public async Task<Result<BudgetResponse>> CreateAsync(CreateBudgetRequest request)
    {
        bool categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
        if (!categoryExists)
            return Result<BudgetResponse>.Failure("Category not found.");

        var budget = new Budget
        {
            CategoryId = request.CategoryId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            LimitAmount = request.LimitAmount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Budget created = await _budgetRepository.AddAsync(budget);
        _logger.LogInformation("Created {EntityType} with Id {EntityId}", "Budget", created.Id);

        return Result<BudgetResponse>.Success(MapToResponse(created));
    }

    public async Task<Result<BudgetResponse>> UpdateAsync(int id, UpdateBudgetRequest request)
    {
        Budget? budget = await _budgetRepository.GetByIdAsync(id);
        if (budget is null)
            return Result<BudgetResponse>.Failure("Budget not found.");

        bool categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
        if (!categoryExists)
            return Result<BudgetResponse>.Failure("Category not found.");

        budget.CategoryId = request.CategoryId;
        budget.StartDate = request.StartDate;
        budget.EndDate = request.EndDate;
        budget.LimitAmount = request.LimitAmount;
        budget.UpdatedAt = DateTime.UtcNow;

        await _budgetRepository.UpdateAsync(budget);
        _logger.LogInformation("Updated {EntityType} with Id {EntityId}", "Budget", budget.Id);

        return Result<BudgetResponse>.Success(MapToResponse(budget));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        Budget? budget = await _budgetRepository.GetByIdAsync(id);
        if (budget is null)
            return Result<bool>.Failure("Budget not found.");

        await _budgetRepository.DeleteAsync(budget);
        _logger.LogInformation("Deleted {EntityType} with Id {EntityId}", "Budget", id);

        return Result<bool>.Success(true);
    }

    private static BudgetResponse MapToResponse(Budget budget) =>
        new(budget.Id, budget.CategoryId, budget.StartDate, budget.EndDate, budget.LimitAmount, budget.CreatedAt, budget.UpdatedAt);
}
