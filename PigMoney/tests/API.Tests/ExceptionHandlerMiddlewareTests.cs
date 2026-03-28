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

public class ExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlerMiddleware>> _loggerMock;

    public ExceptionHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandlerMiddleware>>();
    }

    [Fact]
    public async Task CatchesExceptionAndReturns500()
    {
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = _ => throw new InvalidOperationException("Test exception");

        ExceptionHandlerMiddleware middleware = new(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task LogsExceptionWithCorrelationId()
    {
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "test-correlation-id";

        RequestDelegate next = _ => throw new InvalidOperationException("Test exception");

        ExceptionHandlerMiddleware middleware = new(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("test-correlation-id")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ReturnsSanitizedErrorMessage()
    {
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "test-correlation-id";

        RequestDelegate next = _ => throw new InvalidOperationException("Sensitive internal error details");

        ExceptionHandlerMiddleware middleware = new(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.DoesNotContain("Sensitive internal error details", responseBody);
        Assert.Contains("test-correlation-id", responseBody);
    }
}
