//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Routes;

using Application.DTOs.Categories;
using Application.Services;

public static class CategoryRoutes
{
    public static void MapCategoryRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories");

        group.MapPost("/", async (CreateCategoryRequest request, ICategoryService service) =>
        {
            var result = await service.CreateCategoryAsync(request);
            return result.IsSuccess
                ? Results.Created($"/api/v1/categories/{result.Value.Id}", result.Value)
                : Results.BadRequest(result.Error);
        });

        group.MapGet("/{id:int}", async (int id, ICategoryService service) =>
        {
            var result = await service.GetCategoryByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapGet("/", async (int page, int pageSize, ICategoryService service) =>
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 50 : pageSize;
            var result = await service.GetAllCategoriesAsync(page, pageSize);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPut("/{id:int}", async (int id, UpdateCategoryRequest request, ICategoryService service) =>
        {
            var result = await service.UpdateCategoryAsync(id, request);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapDelete("/{id:int}", async (int id, ICategoryService service) =>
        {
            var result = await service.DeleteCategoryAsync(id);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}
