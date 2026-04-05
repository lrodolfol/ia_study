//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Common;
using Application.DTOs.Expenses;
using Domain.Common;

namespace Application.Services;

public interface IExpenseService
{
    Task<Result<PaginatedList<ExpenseResponse>>> GetAllAsync(ExpenseFilterParams filters, int page, int pageSize);
    Task<Result<ExpenseResponse>> GetByIdAsync(int id);
    Task<Result<ExpenseResponse>> CreateAsync(CreateExpenseRequest request);
    Task<Result<ExpenseResponse>> UpdateAsync(int id, UpdateExpenseRequest request);
    Task<Result<bool>> DeleteAsync(int id);
}
