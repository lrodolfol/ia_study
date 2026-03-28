//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Routes;

using Application.DTOs.ExpenseItems;
using Application.Services;

public static class ExpenseItemRoutes
{
    public static void MapExpenseItemRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/expense-items");

        group.MapPost("/", async (CreateExpenseItemRequest request, IExpenseItemService service) =>
        {
            var result = await service.CreateExpenseItemAsync(request);
            return result.IsSuccess
                ? Results.Created($"/api/v1/expense-items/{result.Value.Id}", result.Value)
                : Results.BadRequest(result.Error);
        });

        group.MapGet("/{id:int}", async (int id, IExpenseItemService service) =>
        {
            var result = await service.GetExpenseItemByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapGet("/by-expense/{expenseId:int}", async (int expenseId, int page, int pageSize, IExpenseItemService service) =>
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 50 : pageSize;
            var result = await service.GetExpenseItemsByExpenseIdAsync(expenseId, page, pageSize);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPut("/{id:int}", async (int id, UpdateExpenseItemRequest request, IExpenseItemService service) =>
        {
            var result = await service.UpdateExpenseItemAsync(id, request);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapDelete("/{id:int}", async (int id, IExpenseItemService service) =>
        {
            var result = await service.DeleteExpenseItemAsync(id);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}
