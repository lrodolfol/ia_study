//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Net;
using System.Text.Json;
using API.Models;

namespace API.Middleware;

public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var errorDetails = context.RequestServices
                .GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                ? new[] { ex.Message }
                : new[] { "An internal server error has occurred." };

            var response = new StandardApiResponse<object>(
                StatusCode: 500,
                Message: "An unexpected error occurred.",
                Error: errorDetails,
                Data: default
            );

            var json = JsonSerializer.Serialize(response, JsonOptions);
            await context.Response.WriteAsync(json);
        }
    }
}
