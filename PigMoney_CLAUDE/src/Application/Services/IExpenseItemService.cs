//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Common;
using Application.DTOs.ExpenseItems;
using Domain.Common;

namespace Application.Services;

public interface IExpenseItemService
{
    Task<Result<PaginatedList<ExpenseItemResponse>>> GetAllByExpenseIdAsync(int expenseId, int page, int pageSize);
    Task<Result<ExpenseItemResponse>> GetByIdAsync(int id);
    Task<Result<ExpenseItemResponse>> CreateAsync(int expenseId, CreateExpenseItemRequest request);
    Task<Result<ExpenseItemResponse>> UpdateAsync(int id, UpdateExpenseItemRequest request);
    Task<Result<bool>> DeleteAsync(int id);
}
