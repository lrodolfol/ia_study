//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Routes;

using Application.DTOs.Expenses;
using Application.Services;

public static class ExpenseRoutes
{
    public static void MapExpenseRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/expenses");

        group.MapPost("/", async (CreateExpenseRequest request, IExpenseService service) =>
        {
            var result = await service.CreateExpenseAsync(request);
            return result.IsSuccess
                ? Results.Created($"/api/v1/expenses/{result.Value.Id}", result.Value)
                : Results.BadRequest(result.Error);
        });

        group.MapGet("/{id:int}", async (int id, IExpenseService service) =>
        {
            var result = await service.GetExpenseByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapGet("/", async (
            DateTime? startDate, 
            DateTime? endDate, 
            int? categoryId, 
            int? accountId, 
            int page, 
            int pageSize, 
            IExpenseService service) =>
        {
            var filters = new ExpenseFilterParams(
                StartDate: startDate,
                EndDate: endDate,
                CategoryId: categoryId,
                AccountId: accountId,
                Page: page <= 0 ? 1 : page,
                PageSize: pageSize <= 0 ? 50 : pageSize
            );
            var result = await service.GetExpensesAsync(filters);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPut("/{id:int}", async (int id, UpdateExpenseRequest request, IExpenseService service) =>
        {
            var result = await service.UpdateExpenseAsync(id, request);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapDelete("/{id:int}", async (int id, IExpenseService service) =>
        {
            var result = await service.DeleteExpenseAsync(id);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}
