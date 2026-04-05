//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace pigMoney.Tests.Integration;

public class ExceptionHandlerMiddlewareTests : IClassFixture<ExceptionThrowingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExceptionHandlerMiddlewareTests(ExceptionThrowingWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UnhandledException_ShouldReturn500WithEnvelope()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/force-error");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.True(root.TryGetProperty("statusCode", out JsonElement statusCode));
        Assert.Equal(500, statusCode.GetInt32());
        Assert.True(root.TryGetProperty("message", out _));
        Assert.True(root.TryGetProperty("error", out JsonElement error));
        Assert.True(error.GetArrayLength() > 0);
        Assert.True(root.TryGetProperty("data", out JsonElement data));
        Assert.Equal(JsonValueKind.Null, data.ValueKind);
    }
}

public class ExceptionThrowingWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<Repository.Data.AppDbContext>)
                         || d.ServiceType == typeof(DbContextOptions)
                         || d.ServiceType == typeof(Repository.Data.AppDbContext))
                .ToList();
            foreach (ServiceDescriptor descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            var internalProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddSingleton<DbContextOptions<Repository.Data.AppDbContext>>(_ =>
                new DbContextOptionsBuilder<Repository.Data.AppDbContext>()
                    .UseInMemoryDatabase("ExceptionTestDb_" + Guid.NewGuid())
                    .UseInternalServiceProvider(internalProvider)
                    .Options);

            services.AddScoped<Repository.Data.AppDbContext>();
        });

        builder.Configure(app =>
        {
            app.UseMiddleware<global::API.Middleware.ExceptionHandlerMiddleware>();
            app.UseMiddleware<global::API.Middleware.ResponseWrapperMiddleware>();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/api/v1/force-error", (HttpContext _) =>
                {
                    throw new InvalidOperationException("Forced test exception");
                });
            });
        });

        builder.UseEnvironment("Development");
    }
}
