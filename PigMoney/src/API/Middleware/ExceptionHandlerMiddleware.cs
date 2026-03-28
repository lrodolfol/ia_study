//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Middleware;

using System.Net;
using System.Text.Json;
using API.Models;

public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            string correlationId = context.TraceIdentifier;

            logger.LogError(
                ex,
                "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
                correlationId,
                context.Request.Path,
                context.Request.Method);

            await HandleExceptionAsync(context, correlationId);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, string correlationId)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        StandardApiResponse<object> response = StandardApiResponse<object>.Failure(
            $"An unexpected error occurred. Please contact support with reference: {correlationId}",
            "Internal Server Error",
            500);

        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }
}

public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
