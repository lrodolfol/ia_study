//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Services;

using Application.DTOs.Categories;
using Application.DTOs.Common;
using Domain.Common;

public interface ICategoryService
{
    Task<Result<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request);
    Task<Result<PaginatedList<CategoryResponse>>> GetAllCategoriesAsync(int page, int pageSize);
    Task<Result<CategoryResponse>> GetCategoryByIdAsync(int id);
    Task<Result<CategoryResponse>> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
    Task<Result> DeleteCategoryAsync(int id);
}
