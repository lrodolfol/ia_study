//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Text.Json;
using API.Models;

namespace API.Middleware;

public class ResponseWrapperMiddleware(RequestDelegate next, ILogger<ResponseWrapperMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ResponseWrapperMiddleware> _logger = logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsSwaggerRequest(context))
        {
            await _next(context);
            return;
        }

        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        memoryStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(memoryStream, leaveOpen: true);
        var responseBody = await reader.ReadToEndAsync();
        memoryStream.Seek(0, SeekOrigin.Begin);

        context.Response.Body = originalBodyStream;

        if (IsAlreadyWrapped(responseBody))
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(responseBody);
            return;
        }

        int statusCode = context.Response.StatusCode;
        string message = GetMessageForStatusCode(statusCode);
        object? data = DeserializeBody(responseBody);
        IEnumerable<string> errors = ExtractErrors(statusCode, data);

        if (errors.Any())
            data = default;

        var wrapped = new StandardApiResponse<object>(statusCode, message, errors, data);
        var json = JsonSerializer.Serialize(wrapped, JsonOptions);

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(json);
    }

    private static bool IsSwaggerRequest(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        return path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsAlreadyWrapped(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            return root.ValueKind == JsonValueKind.Object
                   && root.TryGetProperty("statusCode", out _)
                   && root.TryGetProperty("message", out _)
                   && root.TryGetProperty("error", out _);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse response body for wrapper check");
            return false;
        }
    }

    private object? DeserializeBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return default;

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(body);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize response body");
            return body;
        }
    }

    private static string GetMessageForStatusCode(int statusCode) => statusCode switch
    {
        >= 200 and < 300 => "Success",
        400 => "Validation failed",
        404 => "Resource not found",
        409 => "Conflict",
        500 => "Internal server error",
        _ => "Request processed"
    };

    private static IEnumerable<string> ExtractErrors(int statusCode, object? data)
    {
        if (statusCode < 400)
            return Enumerable.Empty<string>();

        if (data is not JsonElement jsonElement)
            return Enumerable.Empty<string>();

        if (statusCode == 400)
            return ExtractValidationErrors(jsonElement);

        if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("message", out JsonElement msgProp))
            return new[] { msgProp.GetString() ?? "An error occurred" };

        if (jsonElement.ValueKind == JsonValueKind.String)
            return new[] { jsonElement.GetString() ?? "An error occurred" };

        return new[] { "An error occurred" };
    }

    private static IEnumerable<string> ExtractValidationErrors(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("errors", out JsonElement errorsElement))
        {
            var messages = new List<string>();

            if (errorsElement.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in errorsElement.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement errorMsg in property.Value.EnumerateArray())
                        {
                            var msg = errorMsg.GetString();
                            if (!string.IsNullOrEmpty(msg))
                                messages.Add(msg);
                        }
                    }
                }
            }

            if (messages.Count > 0)
                return messages;
        }

        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("title", out JsonElement titleProp))
            return new[] { titleProp.GetString() ?? "Validation failed" };

        return new[] { "Validation failed" };
    }
}
