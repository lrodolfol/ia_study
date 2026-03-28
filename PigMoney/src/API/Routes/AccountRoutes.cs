//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Routes;

using Application.DTOs.Accounts;
using Application.Services;

public static class AccountRoutes
{
    public static void MapAccountRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/accounts");

        group.MapPost("/", async (CreateAccountRequest request, IAccountService service) =>
        {
            var result = await service.CreateAccountAsync(request);
            return result.IsSuccess
                ? Results.Created($"/api/v1/accounts/{result.Value.Id}", result.Value)
                : Results.BadRequest(result.Error);
        });

        group.MapGet("/{id:int}", async (int id, IAccountService service) =>
        {
            var result = await service.GetAccountByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapGet("/", async (int page, int pageSize, IAccountService service) =>
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 50 : pageSize;
            var result = await service.GetAllAccountsAsync(page, pageSize);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPut("/{id:int}", async (int id, UpdateAccountRequest request, IAccountService service) =>
        {
            var result = await service.UpdateAccountAsync(id, request);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapDelete("/{id:int}", async (int id, IAccountService service) =>
        {
            var result = await service.DeleteAccountAsync(id);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}
