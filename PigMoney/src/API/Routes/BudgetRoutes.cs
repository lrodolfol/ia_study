//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Routes;

using Application.DTOs.Budgets;
using Application.Services;

public static class BudgetRoutes
{
    public static void MapBudgetRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/budgets");

        group.MapPost("/", async (CreateBudgetRequest request, IBudgetService service) =>
        {
            var result = await service.CreateBudgetAsync(request);
            return result.IsSuccess
                ? Results.Created($"/api/v1/budgets/{result.Value.Id}", result.Value)
                : Results.BadRequest(result.Error);
        });

        group.MapGet("/{id:int}", async (int id, IBudgetService service) =>
        {
            var result = await service.GetBudgetByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapGet("/", async (int page, int pageSize, IBudgetService service) =>
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 50 : pageSize;
            var result = await service.GetAllBudgetsAsync(page, pageSize);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/by-category/{categoryId:int}", async (int categoryId, int page, int pageSize, IBudgetService service) =>
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 50 : pageSize;
            var result = await service.GetBudgetsByCategoryAsync(categoryId, page, pageSize);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPut("/{id:int}", async (int id, UpdateBudgetRequest request, IBudgetService service) =>
        {
            var result = await service.UpdateBudgetAsync(id, request);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapDelete("/{id:int}", async (int id, IBudgetService service) =>
        {
            var result = await service.DeleteBudgetAsync(id);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}
