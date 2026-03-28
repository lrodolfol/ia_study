//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Services;

using Application.DTOs.Common;
using Application.DTOs.ExpenseItems;
using Domain.Common;

public interface IExpenseItemService
{
    Task<Result<ExpenseItemResponse>> CreateExpenseItemAsync(CreateExpenseItemRequest request);
    Task<Result<PaginatedList<ExpenseItemResponse>>> GetExpenseItemsByExpenseIdAsync(int expenseId, int page, int pageSize);
    Task<Result<ExpenseItemResponse>> GetExpenseItemByIdAsync(int id);
    Task<Result<ExpenseItemResponse>> UpdateExpenseItemAsync(int id, UpdateExpenseItemRequest request);
    Task<Result> DeleteExpenseItemAsync(int id);
}
