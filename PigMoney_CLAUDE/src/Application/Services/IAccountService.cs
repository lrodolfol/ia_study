//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Accounts;
using Application.DTOs.Common;
using Domain.Common;

namespace Application.Services;

public interface IAccountService
{
    Task<Result<PaginatedList<AccountResponse>>> GetAllAsync(int page, int pageSize);
    Task<Result<AccountResponse>> GetByIdAsync(int id);
    Task<Result<AccountResponse>> CreateAsync(CreateAccountRequest request);
    Task<Result<AccountResponse>> UpdateAsync(int id, UpdateAccountRequest request);
    Task<Result<bool>> DeleteAsync(int id);
}
