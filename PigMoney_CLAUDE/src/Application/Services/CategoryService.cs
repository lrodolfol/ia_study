//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Categories;
using Application.DTOs.Common;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger) : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ILogger<CategoryService> _logger = logger;

    public async Task<Result<PaginatedList<CategoryResponse>>> GetAllAsync(int page, int pageSize)
    {
        var categories = await _categoryRepository.GetPagedAsync(page, pageSize);
        int totalCount = await _categoryRepository.CountAsync();

        var items = categories.Select(MapToResponse);

        return Result<PaginatedList<CategoryResponse>>.Success(
            new PaginatedList<CategoryResponse>(items, totalCount, page, pageSize));
    }

    public async Task<Result<CategoryResponse>> GetByIdAsync(int id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
            return Result<CategoryResponse>.Failure("Category not found.");

        return Result<CategoryResponse>.Success(MapToResponse(category));
    }

    public async Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Category created = await _categoryRepository.AddAsync(category);
        _logger.LogInformation("Created {EntityType} with Id {EntityId}", "Category", created.Id);

        return Result<CategoryResponse>.Success(MapToResponse(created));
    }

    public async Task<Result<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
            return Result<CategoryResponse>.Failure("Category not found.");

        category.Name = request.Name;
        category.Description = request.Description;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category);
        _logger.LogInformation("Updated {EntityType} with Id {EntityId}", "Category", category.Id);

        return Result<CategoryResponse>.Success(MapToResponse(category));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
            return Result<bool>.Failure("Category not found.");

        if (category.Expenses.Count > 0 || category.Budgets.Count > 0)
        {
            int dependentCount = category.Expenses.Count + category.Budgets.Count;
            _logger.LogWarning("Delete blocked for {EntityType} {EntityId}: {DependentCount} dependents",
                "Category", id, dependentCount);
            return Result<bool>.Failure("Cannot delete category with linked expenses or budgets.");
        }

        await _categoryRepository.DeleteAsync(category);
        _logger.LogInformation("Deleted {EntityType} with Id {EntityId}", "Category", id);

        return Result<bool>.Success(true);
    }

    private static CategoryResponse MapToResponse(Category category) =>
        new(category.Id, category.Name, category.Description, category.CreatedAt, category.UpdatedAt);
}
