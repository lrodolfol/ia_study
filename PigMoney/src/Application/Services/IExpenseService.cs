//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Services;

using Application.DTOs.Common;
using Application.DTOs.Expenses;
using Domain.Common;

public interface IExpenseService
{
    Task<Result<ExpenseResponse>> CreateExpenseAsync(CreateExpenseRequest request);
    Task<Result<PaginatedList<ExpenseResponse>>> GetExpensesAsync(ExpenseFilterParams filters);
    Task<Result<ExpenseResponse>> GetExpenseByIdAsync(int id);
    Task<Result<ExpenseResponse>> UpdateExpenseAsync(int id, UpdateExpenseRequest request);
    Task<Result> DeleteExpenseAsync(int id);
}
