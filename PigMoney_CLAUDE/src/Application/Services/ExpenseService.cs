//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Common;
using Application.DTOs.Expenses;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ExpenseService(
    IExpenseRepository expenseRepository,
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    IExpenseItemRepository expenseItemRepository,
    ILogger<ExpenseService> logger) : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository = expenseRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IExpenseItemRepository _expenseItemRepository = expenseItemRepository;
    private readonly ILogger<ExpenseService> _logger = logger;

    public async Task<Result<PaginatedList<ExpenseResponse>>> GetAllAsync(ExpenseFilterParams filters, int page, int pageSize)
    {
        var expenses = await _expenseRepository.GetFilteredAsync(filters, page, pageSize);
        int totalCount = await _expenseRepository.CountFilteredAsync(filters);

        var items = expenses.Select(MapToResponse);

        return Result<PaginatedList<ExpenseResponse>>.Success(
            new PaginatedList<ExpenseResponse>(items, totalCount, page, pageSize));
    }

    public async Task<Result<ExpenseResponse>> GetByIdAsync(int id)
    {
        Expense? expense = await _expenseRepository.GetByIdAsync(id);
        if (expense is null)
            return Result<ExpenseResponse>.Failure("Expense not found.");

        return Result<ExpenseResponse>.Success(MapToResponse(expense));
    }

    public async Task<Result<ExpenseResponse>> CreateAsync(CreateExpenseRequest request)
    {
        bool accountExists = await _accountRepository.ExistsAsync(request.AccountId);
        if (!accountExists)
            return Result<ExpenseResponse>.Failure("Account not found.");

        bool categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
        if (!categoryExists)
            return Result<ExpenseResponse>.Failure("Category not found.");

        var expense = new Expense
        {
            Amount = request.Amount,
            Date = request.Date,
            Description = request.Description,
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Expense created = await _expenseRepository.AddAsync(expense);
        _logger.LogInformation("Created {EntityType} with Id {EntityId}", "Expense", created.Id);

        return Result<ExpenseResponse>.Success(MapToResponse(created));
    }

    public async Task<Result<ExpenseResponse>> UpdateAsync(int id, UpdateExpenseRequest request)
    {
        Expense? expense = await _expenseRepository.GetByIdAsync(id);
        if (expense is null)
            return Result<ExpenseResponse>.Failure("Expense not found.");

        bool accountExists = await _accountRepository.ExistsAsync(request.AccountId);
        if (!accountExists)
            return Result<ExpenseResponse>.Failure("Account not found.");

        bool categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
        if (!categoryExists)
            return Result<ExpenseResponse>.Failure("Category not found.");

        expense.Amount = request.Amount;
        expense.Date = request.Date;
        expense.Description = request.Description;
        expense.AccountId = request.AccountId;
        expense.CategoryId = request.CategoryId;
        expense.UpdatedAt = DateTime.UtcNow;

        await _expenseRepository.UpdateAsync(expense);
        _logger.LogInformation("Updated {EntityType} with Id {EntityId}", "Expense", expense.Id);

        return Result<ExpenseResponse>.Success(MapToResponse(expense));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        Expense? expense = await _expenseRepository.GetByIdAsync(id);
        if (expense is null)
            return Result<bool>.Failure("Expense not found.");

        int itemCount = await _expenseItemRepository.CountByExpenseIdAsync(id);
        if (itemCount > 0)
        {
            _logger.LogWarning("Delete blocked for {EntityType} with Id {EntityId} — has {DependentCount} items", "Expense", id, itemCount);
            return Result<bool>.Failure("Cannot delete expense with existing items.");
        }

        await _expenseRepository.DeleteAsync(expense);
        _logger.LogInformation("Deleted {EntityType} with Id {EntityId}", "Expense", id);

        return Result<bool>.Success(true);
    }

    private static ExpenseResponse MapToResponse(Expense expense) =>
        new(expense.Id, expense.Amount, expense.Date, expense.Description, expense.AccountId, expense.CategoryId, expense.CreatedAt, expense.UpdatedAt);
}
