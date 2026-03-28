//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Middleware;

using System.Text.Json;
using API.Models;

public class ResponseWrapperMiddleware(RequestDelegate next, ILogger<ResponseWrapperMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        Stream originalBodyStream = context.Response.Body;

        using MemoryStream responseBody = new();
        context.Response.Body = responseBody;

        await next(context);

        context.Response.Body = originalBodyStream;

        responseBody.Seek(0, SeekOrigin.Begin);
        string responseContent = await new StreamReader(responseBody).ReadToEndAsync();

        if (ShouldWrapResponse(context))
        {
            await WrapResponseAsync(context, responseContent);
        }
        else
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private static bool ShouldWrapResponse(HttpContext context)
    {
        string? contentType = context.Response.ContentType;

        if (string.IsNullOrEmpty(contentType))
        {
            return true;
        }

        return contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
               !contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task WrapResponseAsync(HttpContext context, string responseContent)
    {
        int statusCode = context.Response.StatusCode;
        object? data = null;
        List<string> errors = [];
        string message = GetMessageForStatusCode(statusCode);

        if (!string.IsNullOrWhiteSpace(responseContent))
        {
            try
            {
                data = JsonSerializer.Deserialize<object>(responseContent, JsonOptions);
            }
            catch (JsonException)
            {
                data = responseContent;
            }
        }

        if (statusCode >= 400)
        {
            if (data is JsonElement element && element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("errors", out JsonElement errorsElement))
                {
                    errors = ExtractErrors(errorsElement);
                    data = null;
                }
                else if (element.TryGetProperty("error", out JsonElement errorElement))
                {
                    errors = [errorElement.GetString() ?? "Unknown error"];
                    data = null;
                }
            }
            else if (data is string errorString)
            {
                errors = [errorString];
                data = null;
            }
        }

        StandardApiResponse<object> response = new(data, errors, message, statusCode);

        context.Response.ContentType = "application/json";
        string json = JsonSerializer.Serialize(response, JsonOptions);

        await context.Response.WriteAsync(json);
    }

    private static List<string> ExtractErrors(JsonElement errorsElement)
    {
        List<string> errors = [];

        if (errorsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement error in errorsElement.EnumerateArray())
            {
                if (error.ValueKind == JsonValueKind.String)
                {
                    errors.Add(error.GetString() ?? string.Empty);
                }
            }
        }
        else if (errorsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in errorsElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement errorItem in property.Value.EnumerateArray())
                    {
                        errors.Add($"{property.Name}: {errorItem.GetString()}");
                    }
                }
            }
        }

        return errors;
    }

    private static string GetMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            200 => "Success",
            201 => "Created",
            204 => "No Content",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            422 => "Validation Error",
            500 => "Internal Server Error",
            _ => "Unknown"
        };
    }
}

public static class ResponseWrapperMiddlewareExtensions
{
    public static IApplicationBuilder UseResponseWrapperMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ResponseWrapperMiddleware>();
    }
}
