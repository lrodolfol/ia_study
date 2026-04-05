//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Common;
using Application.DTOs.Incomes;
using Domain.Common;

namespace Application.Services;

public interface IIncomeService
{
    Task<Result<PaginatedList<IncomeResponse>>> GetAllAsync(IncomeFilterParams filters, int page, int pageSize);
    Task<Result<IncomeResponse>> GetByIdAsync(int id);
    Task<Result<IncomeResponse>> CreateAsync(CreateIncomeRequest request);
    Task<Result<IncomeResponse>> UpdateAsync(int id, UpdateIncomeRequest request);
    Task<Result<bool>> DeleteAsync(int id);
}
