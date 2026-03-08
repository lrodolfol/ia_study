using System.Net.Http.Json;
using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Services;

public class WeatherService(
    HttpClient httpClient,
    IWeatherCityRepository repository,
    IConfiguration configuration,
    ILogger<WeatherService> logger) : IWeatherService
{
    private const int MaxRetries = 3;

    public async Task<WeatherResponseDto> GetWeatherByCityAsync(string city)
    {
        var apiKey = configuration["WeatherApi:ApiKey"];
        var requestPath = $"current.json?key={apiKey}&q={Uri.EscapeDataString(city)}&aqi=no&pollen=no";
        var fullRequestUrl = new Uri(httpClient.BaseAddress!, requestPath).ToString();

        logger.LogInformation("Fetching weather data for city: {City}", city);

        HttpResponseMessage response = null!;
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                response = await httpClient.GetAsync(requestPath);
                break;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !ex.CancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Timeout on attempt {Attempt}/{MaxRetries} for city: {City}", attempt, MaxRetries, city);
                if (attempt == MaxRetries)
                    throw;
            }
        }

        response.EnsureSuccessStatusCode();

        var apiResponse = await response.Content.ReadFromJsonAsync<WeatherApiResponseDto>()
            ?? throw new InvalidOperationException("Failed to deserialize weather API response.");

        await repository.SaveAsync(new WeatherCity { Request = fullRequestUrl });

        logger.LogInformation("Weather data saved for city: {City}", city);

        return new WeatherResponseDto(
            new LocationResponseDto(
                apiResponse.Location.Name,
                apiResponse.Location.Region,
                apiResponse.Location.Country,
                apiResponse.Location.Lat,
                apiResponse.Location.Lon,
                apiResponse.Location.TzId,
                apiResponse.Location.LocalTime
            ),
            new CurrentWeatherResponseDto(
                apiResponse.Current.TempC,
                apiResponse.Current.TempF,
                apiResponse.Current.IsDay == 1,
                new ConditionResponseDto(
                    apiResponse.Current.Condition.Text,
                    apiResponse.Current.Condition.Icon,
                    apiResponse.Current.Condition.Code
                ),
                apiResponse.Current.WindMph,
                apiResponse.Current.WindKph,
                apiResponse.Current.WindDegree,
                apiResponse.Current.WindDir,
                apiResponse.Current.PressureMb,
                apiResponse.Current.PrecipMm,
                apiResponse.Current.Humidity,
                apiResponse.Current.Cloud,
                apiResponse.Current.FeelsLikeC,
                apiResponse.Current.FeelsLikeF,
                apiResponse.Current.Uv
            )
        );
    }
}
