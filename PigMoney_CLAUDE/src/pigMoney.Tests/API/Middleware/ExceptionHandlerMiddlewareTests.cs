//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Text.Json;
using API.Middleware;
using API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace pigMoney.Tests.API.Middleware;

public class ExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlerMiddleware>> _loggerMock = new();

    private static HttpContext CreateHttpContext(bool isDevelopment)
    {
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns(isDevelopment ? "Development" : "Production");

        var services = new ServiceCollection();
        services.AddSingleton(envMock.Object);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ExceptionHandlerMiddleware(next, _loggerMock.Object);
        HttpContext context = CreateHttpContext(isDevelopment: true);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldReturn500()
    {
        RequestDelegate next = _ => throw new InvalidOperationException("Test error");

        var middleware = new ExceptionHandlerMiddleware(next, _loggerMock.Object);
        HttpContext context = CreateHttpContext(isDevelopment: true);

        await middleware.InvokeAsync(context);

        Assert.Equal(500, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldReturnJsonContentType()
    {
        RequestDelegate next = _ => throw new Exception("fail");

        var middleware = new ExceptionHandlerMiddleware(next, _loggerMock.Object);
        HttpContext context = CreateHttpContext(isDevelopment: true);

        await middleware.InvokeAsync(context);

        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionInDevelopment_ShouldExposeExceptionMessage()
    {
        RequestDelegate next = _ => throw new InvalidOperationException("Something broke");

        var middleware = new ExceptionHandlerMiddleware(next, _loggerMock.Object);
        HttpContext context = CreateHttpContext(isDevelopment: true);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        StandardApiResponse<object>? response = JsonSerializer.Deserialize<StandardApiResponse<object>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(response);
        Assert.Equal(500, response.StatusCode);
        Assert.Equal("An unexpected error occurred.", response.Message);
        Assert.Contains("Something broke", response.Error);
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionInProduction_ShouldNotExposeExceptionMessage()
    {
        RequestDelegate next = _ => throw new InvalidOperationException("Secret internal error");

        var middleware = new ExceptionHandlerMiddleware(next, _loggerMock.Object);
        HttpContext context = CreateHttpContext(isDevelopment: false);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        StandardApiResponse<object>? response = JsonSerializer.Deserialize<StandardApiResponse<object>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(response);
        Assert.Equal(500, response.StatusCode);
        Assert.DoesNotContain("Secret internal error", response.Error);
        Assert.Contains("An internal server error has occurred.", response.Error);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldLogError()
    {
        RequestDelegate next = _ => throw new Exception("logged error");

        var middleware = new ExceptionHandlerMiddleware(next, _loggerMock.Object);
        HttpContext context = CreateHttpContext(isDevelopment: true);

        await middleware.InvokeAsync(context);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
