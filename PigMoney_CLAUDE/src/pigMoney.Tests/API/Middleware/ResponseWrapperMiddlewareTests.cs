//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Text;
using System.Text.Json;
using API.Middleware;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace pigMoney.Tests.API.Middleware;

public class ResponseWrapperMiddlewareTests
{
    private readonly Mock<ILogger<ResponseWrapperMiddleware>> _loggerMock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task InvokeAsync_WhenOk_ShouldWrapResponseInEnvelope()
    {
        var responseData = new { Id = 1, Name = "Test" };
        RequestDelegate next = async ctx =>
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(responseData, JsonOptions);
            await ctx.Response.WriteAsync(json);
        };

        var middleware = new ResponseWrapperMiddleware(next, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        using var doc = JsonDocument.Parse(body);
        JsonElement root = doc.RootElement;

        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Success", root.GetProperty("message").GetString());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("error").ValueKind);
        Assert.Equal(0, root.GetProperty("error").GetArrayLength());
        Assert.Equal(1, root.GetProperty("data").GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task InvokeAsync_WhenCreated_ShouldWrapWith201()
    {
        RequestDelegate next = async ctx =>
        {
            ctx.Response.StatusCode = 201;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync("{\"id\":5}");
        };

        var middleware = new ResponseWrapperMiddleware(next, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        using var doc = JsonDocument.Parse(body);
        JsonElement root = doc.RootElement;

        Assert.Equal(201, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Success", root.GetProperty("message").GetString());
    }

    [Fact]
    public async Task InvokeAsync_WhenNoContent_ShouldWrapWithNullData()
    {
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 204;
            return Task.CompletedTask;
        };

        var middleware = new ResponseWrapperMiddleware(next, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        using var doc = JsonDocument.Parse(body);
        JsonElement root = doc.RootElement;

        Assert.Equal(204, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Success", root.GetProperty("message").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
    }

    [Fact]
    public async Task InvokeAsync_When404_ShouldWrapWithNotFoundMessage()
    {
        RequestDelegate next = async ctx =>
        {
            ctx.Response.StatusCode = 404;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync("\"Account not found\"");
        };

        var middleware = new ResponseWrapperMiddleware(next, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        using var doc = JsonDocument.Parse(body);
        JsonElement root = doc.RootElement;

        Assert.Equal(404, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Resource not found", root.GetProperty("message").GetString());
        Assert.True(root.GetProperty("error").GetArrayLength() > 0);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
    }

    [Fact]
    public async Task InvokeAsync_WhenAlreadyWrapped_ShouldNotDoubleWrap()
    {
        var wrapped = new StandardApiResponse<object>(500, "An unexpected error occurred.", new[] { "error" }, null);
        RequestDelegate next = async ctx =>
        {
            ctx.Response.StatusCode = 500;
            ctx.Response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(wrapped, JsonOptions);
            await ctx.Response.WriteAsync(json);
        };

        var middleware = new ResponseWrapperMiddleware(next, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        using var doc = JsonDocument.Parse(body);
        JsonElement root = doc.RootElement;

        Assert.Equal(500, root.GetProperty("statusCode").GetInt32());
        Assert.False(root.TryGetProperty("data", out JsonElement dataProp) && dataProp.ValueKind == JsonValueKind.Object && dataProp.TryGetProperty("statusCode", out _));
    }

    [Fact]
    public async Task InvokeAsync_WhenSwaggerPath_ShouldNotWrap()
    {
        var called = false;
        RequestDelegate next = ctx =>
        {
            called = true;
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        };

        var middleware = new ResponseWrapperMiddleware(next, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger/index.html";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_When400ValidationError_ShouldExtractErrors()
    {
        RequestDelegate next = async ctx =>
        {
            ctx.Response.StatusCode = 400;
            ctx.Response.ContentType = "application/json";
            var problemDetails = new
            {
                title = "One or more validation errors occurred.",
                errors = new Dictionary<string, string[]>
                {
                    { "Name", new[] { "Name is required" } },
                    { "Amount", new[] { "Amount must be greater than 0" } }
                }
            };
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
        };

        var middleware = new ResponseWrapperMiddleware(next, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        using var doc = JsonDocument.Parse(body);
        JsonElement root = doc.RootElement;

        Assert.Equal(400, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Validation failed", root.GetProperty("message").GetString());
        JsonElement errors = root.GetProperty("error");
        Assert.True(errors.GetArrayLength() >= 2);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
    }

    private static async Task<string> ReadResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await new StreamReader(context.Response.Body).ReadToEndAsync();
    }
}
