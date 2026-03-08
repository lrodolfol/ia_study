using API.Data;
using API.Entities;
using API.Interfaces;

namespace API.Repositories;

public class WeatherCityRepository(AppDbContext context) : IWeatherCityRepository
{
    public async Task SaveAsync(WeatherCity entity)
    {
        context.WeatherCities.Add(entity);
        await context.SaveChangesAsync();
    }
}
