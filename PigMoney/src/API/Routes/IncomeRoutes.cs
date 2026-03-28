//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Routes;

using Application.DTOs.Incomes;
using Application.Services;

public static class IncomeRoutes
{
    public static void MapIncomeRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/incomes");

        group.MapPost("/", async (CreateIncomeRequest request, IIncomeService service) =>
        {
            var result = await service.CreateIncomeAsync(request);
            return result.IsSuccess
                ? Results.Created($"/api/v1/incomes/{result.Value.Id}", result.Value)
                : Results.BadRequest(result.Error);
        });

        group.MapGet("/{id:int}", async (int id, IIncomeService service) =>
        {
            var result = await service.GetIncomeByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapGet("/", async (
            DateTime? startDate, 
            DateTime? endDate, 
            int? categoryId, 
            int? accountId, 
            int page, 
            int pageSize, 
            IIncomeService service) =>
        {
            var filters = new IncomeFilterParams(
                StartDate: startDate,
                EndDate: endDate,
                CategoryId: categoryId,
                AccountId: accountId,
                Page: page <= 0 ? 1 : page,
                PageSize: pageSize <= 0 ? 50 : pageSize
            );
            var result = await service.GetIncomesAsync(filters);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPut("/{id:int}", async (int id, UpdateIncomeRequest request, IIncomeService service) =>
        {
            var result = await service.UpdateIncomeAsync(id, request);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapDelete("/{id:int}", async (int id, IIncomeService service) =>
        {
            var result = await service.DeleteIncomeAsync(id);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}
