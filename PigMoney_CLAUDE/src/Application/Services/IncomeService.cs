//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Common;
using Application.DTOs.Incomes;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class IncomeService(IIncomeRepository incomeRepository, IAccountRepository accountRepository, ILogger<IncomeService> logger) : IIncomeService
{
    private readonly IIncomeRepository _incomeRepository = incomeRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ILogger<IncomeService> _logger = logger;

    public async Task<Result<PaginatedList<IncomeResponse>>> GetAllAsync(IncomeFilterParams filters, int page, int pageSize)
    {
        var incomes = await _incomeRepository.GetFilteredAsync(filters, page, pageSize);
        int totalCount = await _incomeRepository.CountFilteredAsync(filters);

        var items = incomes.Select(MapToResponse);

        return Result<PaginatedList<IncomeResponse>>.Success(
            new PaginatedList<IncomeResponse>(items, totalCount, page, pageSize));
    }

    public async Task<Result<IncomeResponse>> GetByIdAsync(int id)
    {
        Income? income = await _incomeRepository.GetByIdAsync(id);
        if (income is null)
            return Result<IncomeResponse>.Failure("Income not found.");

        return Result<IncomeResponse>.Success(MapToResponse(income));
    }

    public async Task<Result<IncomeResponse>> CreateAsync(CreateIncomeRequest request)
    {
        bool accountExists = await _accountRepository.ExistsAsync(request.AccountId);
        if (!accountExists)
            return Result<IncomeResponse>.Failure("Account not found.");

        var income = new Income
        {
            Amount = request.Amount,
            Date = request.Date,
            Description = request.Description,
            AccountId = request.AccountId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Income created = await _incomeRepository.AddAsync(income);
        _logger.LogInformation("Created {EntityType} with Id {EntityId}", "Income", created.Id);

        return Result<IncomeResponse>.Success(MapToResponse(created));
    }

    public async Task<Result<IncomeResponse>> UpdateAsync(int id, UpdateIncomeRequest request)
    {
        Income? income = await _incomeRepository.GetByIdAsync(id);
        if (income is null)
            return Result<IncomeResponse>.Failure("Income not found.");

        bool accountExists = await _accountRepository.ExistsAsync(request.AccountId);
        if (!accountExists)
            return Result<IncomeResponse>.Failure("Account not found.");

        income.Amount = request.Amount;
        income.Date = request.Date;
        income.Description = request.Description;
        income.AccountId = request.AccountId;
        income.UpdatedAt = DateTime.UtcNow;

        await _incomeRepository.UpdateAsync(income);
        _logger.LogInformation("Updated {EntityType} with Id {EntityId}", "Income", income.Id);

        return Result<IncomeResponse>.Success(MapToResponse(income));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        Income? income = await _incomeRepository.GetByIdAsync(id);
        if (income is null)
            return Result<bool>.Failure("Income not found.");

        await _incomeRepository.DeleteAsync(income);
        _logger.LogInformation("Deleted {EntityType} with Id {EntityId}", "Income", id);

        return Result<bool>.Success(true);
    }

    private static IncomeResponse MapToResponse(Income income) =>
        new(income.Id, income.Amount, income.Date, income.Description, income.AccountId, income.CreatedAt, income.UpdatedAt);
}
