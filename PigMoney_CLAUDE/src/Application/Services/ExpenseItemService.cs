//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Common;
using Application.DTOs.ExpenseItems;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ExpenseItemService(
    IExpenseItemRepository expenseItemRepository,
    IExpenseRepository expenseRepository,
    ILogger<ExpenseItemService> logger) : IExpenseItemService
{
    private readonly IExpenseItemRepository _expenseItemRepository = expenseItemRepository;
    private readonly IExpenseRepository _expenseRepository = expenseRepository;
    private readonly ILogger<ExpenseItemService> _logger = logger;

    public async Task<Result<PaginatedList<ExpenseItemResponse>>> GetAllByExpenseIdAsync(int expenseId, int page, int pageSize)
    {
        var items = await _expenseItemRepository.GetByExpenseIdAsync(expenseId, page, pageSize);
        int totalCount = await _expenseItemRepository.CountByExpenseIdAsync(expenseId);

        var responses = items.Select(MapToResponse);

        return Result<PaginatedList<ExpenseItemResponse>>.Success(
            new PaginatedList<ExpenseItemResponse>(responses, totalCount, page, pageSize));
    }

    public async Task<Result<ExpenseItemResponse>> GetByIdAsync(int id)
    {
        ExpenseItem? item = await _expenseItemRepository.GetByIdAsync(id);
        if (item is null)
            return Result<ExpenseItemResponse>.Failure("Expense item not found.");

        return Result<ExpenseItemResponse>.Success(MapToResponse(item));
    }

    public async Task<Result<ExpenseItemResponse>> CreateAsync(int expenseId, CreateExpenseItemRequest request)
    {
        bool expenseExists = await _expenseRepository.ExistsAsync(expenseId);
        if (!expenseExists)
            return Result<ExpenseItemResponse>.Failure("Expense not found.");

        var item = new ExpenseItem
        {
            Name = request.Name,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            ExpenseId = expenseId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        ExpenseItem created = await _expenseItemRepository.AddAsync(item);
        _logger.LogInformation("Created {EntityType} with Id {EntityId}", "ExpenseItem", created.Id);

        return Result<ExpenseItemResponse>.Success(MapToResponse(created));
    }

    public async Task<Result<ExpenseItemResponse>> UpdateAsync(int id, UpdateExpenseItemRequest request)
    {
        ExpenseItem? item = await _expenseItemRepository.GetByIdAsync(id);
        if (item is null)
            return Result<ExpenseItemResponse>.Failure("Expense item not found.");

        item.Name = request.Name;
        item.Quantity = request.Quantity;
        item.UnitPrice = request.UnitPrice;
        item.UpdatedAt = DateTime.UtcNow;

        await _expenseItemRepository.UpdateAsync(item);
        _logger.LogInformation("Updated {EntityType} with Id {EntityId}", "ExpenseItem", item.Id);

        return Result<ExpenseItemResponse>.Success(MapToResponse(item));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        ExpenseItem? item = await _expenseItemRepository.GetByIdAsync(id);
        if (item is null)
            return Result<bool>.Failure("Expense item not found.");

        await _expenseItemRepository.DeleteAsync(item);
        _logger.LogInformation("Deleted {EntityType} with Id {EntityId}", "ExpenseItem", id);

        return Result<bool>.Success(true);
    }

    private static ExpenseItemResponse MapToResponse(ExpenseItem item) =>
        new(item.Id, item.Name, item.Quantity, item.UnitPrice, item.ExpenseId, item.CreatedAt, item.UpdatedAt);
}
