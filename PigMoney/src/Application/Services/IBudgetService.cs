//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Services;

using Application.DTOs.Budgets;
using Application.DTOs.Common;
using Domain.Common;

public interface IBudgetService
{
    Task<Result<BudgetResponse>> CreateBudgetAsync(CreateBudgetRequest request);
    Task<Result<PaginatedList<BudgetResponse>>> GetAllBudgetsAsync(int page, int pageSize);
    Task<Result<BudgetResponse>> GetBudgetByIdAsync(int id);
    Task<Result<PaginatedList<BudgetResponse>>> GetBudgetsByCategoryAsync(int categoryId, int page, int pageSize);
    Task<Result<BudgetResponse>> UpdateBudgetAsync(int id, UpdateBudgetRequest request);
    Task<Result> DeleteBudgetAsync(int id);
}
