//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Services;

using Application.DTOs.Categories;
using Application.DTOs.Common;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

public class CategoryService(ICategoryRepository repository, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<Result<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request)
    {
        logger.LogInformation("Creating category with name {Name}", request.Name);

        Category category = new()
        {
            Name = request.Name,
            Type = request.Type
        };

        Result<Category> result = await repository.AddAsync(category);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create category: {Error}", result.Error);
            return Result<CategoryResponse>.Failure(result.Error);
        }

        Result saveResult = await repository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save category: {Error}", saveResult.Error);
            return Result<CategoryResponse>.Failure(saveResult.Error);
        }

        CategoryResponse response = MapToResponse(result.Value!);

        logger.LogInformation("Category created with id {Id}", response.Id);

        return Result<CategoryResponse>.Success(response);
    }

    public async Task<Result<PaginatedList<CategoryResponse>>> GetAllCategoriesAsync(int page, int pageSize)
    {
        logger.LogInformation("Getting categories page {Page} with size {PageSize}", page, pageSize);

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 50;
        }

        Result<IEnumerable<Category>> result = await repository.GetAllAsync(page, pageSize);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to get categories: {Error}", result.Error);
            return Result<PaginatedList<CategoryResponse>>.Failure(result.Error);
        }

        Result<int> countResult = await repository.GetTotalCountAsync();

        if (!countResult.IsSuccess)
        {
            logger.LogError("Failed to get category count: {Error}", countResult.Error);
            return Result<PaginatedList<CategoryResponse>>.Failure(countResult.Error);
        }

        List<CategoryResponse> items = result.Value!.Select(MapToResponse).ToList();

        PaginatedList<CategoryResponse> paginatedList = new(
            items,
            countResult.Value,
            page,
            pageSize);

        return Result<PaginatedList<CategoryResponse>>.Success(paginatedList);
    }

    public async Task<Result<CategoryResponse>> GetCategoryByIdAsync(int id)
    {
        logger.LogInformation("Getting category with id {Id}", id);

        Result<Category> result = await repository.GetByIdAsync(id);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Category with id {Id} not found", id);
            return Result<CategoryResponse>.Failure(result.Error);
        }

        CategoryResponse response = MapToResponse(result.Value!);

        return Result<CategoryResponse>.Success(response);
    }

    public async Task<Result<CategoryResponse>> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        logger.LogInformation("Updating category with id {Id}", id);

        Result<Category> getResult = await repository.GetByIdAsync(id);

        if (!getResult.IsSuccess)
        {
            logger.LogWarning("Category with id {Id} not found", id);
            return Result<CategoryResponse>.Failure(getResult.Error);
        }

        Category category = getResult.Value!;

        if (request.Name is not null)
        {
            category.Name = request.Name;
        }

        if (request.Type.HasValue)
        {
            category.Type = request.Type.Value;
        }

        Result<Category> updateResult = await repository.UpdateAsync(category);

        if (!updateResult.IsSuccess)
        {
            logger.LogError("Failed to update category: {Error}", updateResult.Error);
            return Result<CategoryResponse>.Failure(updateResult.Error);
        }

        Result saveResult = await repository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save category: {Error}", saveResult.Error);
            return Result<CategoryResponse>.Failure(saveResult.Error);
        }

        CategoryResponse response = MapToResponse(updateResult.Value!);

        logger.LogInformation("Category with id {Id} updated", id);

        return Result<CategoryResponse>.Success(response);
    }

    public async Task<Result> DeleteCategoryAsync(int id)
    {
        logger.LogInformation("Deleting category with id {Id}", id);

        Result<Category> getResult = await repository.GetByIdAsync(id);

        if (!getResult.IsSuccess)
        {
            logger.LogWarning("Category with id {Id} not found", id);
            return Result.Failure(getResult.Error);
        }

        Result<bool> hasDependenciesResult = await repository.HasDependenciesAsync(id);

        if (!hasDependenciesResult.IsSuccess)
        {
            logger.LogError("Failed to check category dependencies: {Error}", hasDependenciesResult.Error);
            return Result.Failure(hasDependenciesResult.Error);
        }

        if (hasDependenciesResult.Value)
        {
            logger.LogWarning("Cannot delete category with active transactions");
            return Result.Failure("Cannot delete category with active transactions");
        }

        Result deleteResult = await repository.DeleteAsync(id);

        if (!deleteResult.IsSuccess)
        {
            logger.LogError("Failed to delete category: {Error}", deleteResult.Error);
            return Result.Failure(deleteResult.Error);
        }

        Result saveResult = await repository.SaveChangesAsync();

        if (!saveResult.IsSuccess)
        {
            logger.LogError("Failed to save category deletion: {Error}", saveResult.Error);
            return Result.Failure(saveResult.Error);
        }

        logger.LogInformation("Category with id {Id} deleted", id);

        return Result.Success();
    }

    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Type,
            category.CreatedAt,
            category.UpdatedAt);
    }
}
