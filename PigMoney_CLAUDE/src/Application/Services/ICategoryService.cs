//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Categories;
using Application.DTOs.Common;
using Domain.Common;

namespace Application.Services;

public interface ICategoryService
{
    Task<Result<PaginatedList<CategoryResponse>>> GetAllAsync(int page, int pageSize);
    Task<Result<CategoryResponse>> GetByIdAsync(int id);
    Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request);
    Task<Result<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request);
    Task<Result<bool>> DeleteAsync(int id);
}
