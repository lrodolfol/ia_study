//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository.Data;

namespace pigMoney.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "TestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                         || d.ServiceType == typeof(DbContextOptions)
                         || d.ServiceType == typeof(AppDbContext))
                .ToList();
            foreach (ServiceDescriptor descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            var internalProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddSingleton<DbContextOptions<AppDbContext>>(_ =>
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(_databaseName)
                    .UseInternalServiceProvider(internalProvider)
                    .Options);

            services.AddScoped<AppDbContext>();
        });

        builder.UseEnvironment("Development");
    }
}
