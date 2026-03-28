//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Services;

using Application.DTOs.Common;
using Application.DTOs.Incomes;
using Domain.Common;

public interface IIncomeService
{
    Task<Result<IncomeResponse>> CreateIncomeAsync(CreateIncomeRequest request);
    Task<Result<PaginatedList<IncomeResponse>>> GetIncomesAsync(IncomeFilterParams filters);
    Task<Result<IncomeResponse>> GetIncomeByIdAsync(int id);
    Task<Result<IncomeResponse>> UpdateIncomeAsync(int id, UpdateIncomeRequest request);
    Task<Result> DeleteIncomeAsync(int id);
}
