//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Services;

using Application.DTOs.Accounts;
using Application.DTOs.Common;
using Domain.Common;

public interface IAccountService
{
    Task<Result<AccountResponse>> CreateAccountAsync(CreateAccountRequest request);
    Task<Result<PaginatedList<AccountResponse>>> GetAllAccountsAsync(int page, int pageSize);
    Task<Result<AccountResponse>> GetAccountByIdAsync(int id);
    Task<Result<AccountResponse>> UpdateAccountAsync(int id, UpdateAccountRequest request);
    Task<Result> DeleteAccountAsync(int id);
}
