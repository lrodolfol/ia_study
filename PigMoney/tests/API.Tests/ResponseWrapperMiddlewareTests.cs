//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Tests;

using System.Net;
using System.Text;
using System.Text.Json;
using API.Middleware;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

public class ResponseWrapperMiddlewareTests
{
    private readonly Mock<ILogger<ResponseWrapperMiddleware>> _loggerMock;

    public ResponseWrapperMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ResponseWrapperMiddleware>>();
    }

    [Fact]
    public async Task WrapsSuccessResponseCorrectly()
    {
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();

        object data = new { id = 1, name = "Test" };
        string jsonData = JsonSerializer.Serialize(data);

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync(jsonData);
        };

        ResponseWrapperMiddleware middleware = new(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        JsonDocument doc = JsonDocument.Parse(responseBody);
        JsonElement root = doc.RootElement;

        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Success", root.GetProperty("message").GetString());
        Assert.True(root.GetProperty("errors").GetArrayLength() == 0);
    }

    [Fact]
    public async Task WrapsErrorResponseCorrectly()
    {
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = 400;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync("{\"error\": \"Invalid request\"}");
        };

        ResponseWrapperMiddleware middleware = new(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        JsonDocument doc = JsonDocument.Parse(responseBody);
        JsonElement root = doc.RootElement;

        Assert.Equal(400, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Bad Request", root.GetProperty("message").GetString());
    }

    [Theory]
    [InlineData(200, "Success")]
    [InlineData(201, "Created")]
    [InlineData(400, "Bad Request")]
    [InlineData(404, "Not Found")]
    public async Task SetsCorrectStatusCodes(int statusCode, string expectedMessage)
    {
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = statusCode;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync("{}");
        };

        ResponseWrapperMiddleware middleware = new(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        JsonDocument doc = JsonDocument.Parse(responseBody);
        JsonElement root = doc.RootElement;

        Assert.Equal(statusCode, root.GetProperty("statusCode").GetInt32());
        Assert.Equal(expectedMessage, root.GetProperty("message").GetString());
    }
}
