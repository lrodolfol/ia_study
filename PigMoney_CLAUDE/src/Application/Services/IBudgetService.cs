//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Budgets;
using Application.DTOs.Common;
using Domain.Common;

namespace Application.Services;

public interface IBudgetService
{
    Task<Result<PaginatedList<BudgetResponse>>> GetAllAsync(BudgetFilterParams filters, int page, int pageSize);
    Task<Result<BudgetResponse>> GetByIdAsync(int id);
    Task<Result<BudgetResponse>> CreateAsync(CreateBudgetRequest request);
    Task<Result<BudgetResponse>> UpdateAsync(int id, UpdateBudgetRequest request);
    Task<Result<bool>> DeleteAsync(int id);
}
