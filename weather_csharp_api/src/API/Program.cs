using API.Data;
using API.Interfaces;
using API.Repositories;
using API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:7070");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IWeatherCityRepository, WeatherCityRepository>();

builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WeatherApi:BaseUrl"]!);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API v1"));

var apiGroup = app.MapGroup("/api");

apiGroup.MapGet("/weather/city", async (
    string city,
    IWeatherService weatherService,
    ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(city))
        return Results.BadRequest(new { error = "The 'city' parameter is required." });

    try
    {
        var result = await weatherService.GetWeatherByCityAsync(city);
        return Results.Ok(result);
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error fetching weather data for city: {City}", city);
        return Results.Problem(detail: "Failed to fetch weather data from external API.", statusCode: 500);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error processing weather request for city: {City}", city);
        return Results.Problem(detail: "An unexpected error occurred.", statusCode: 500);
    }
})
.WithName("GetWeatherByCity")
.Produces<API.DTOs.WeatherResponseDto>(200)
.ProducesProblem(400)
.ProducesProblem(500);

await app.RunAsync();
