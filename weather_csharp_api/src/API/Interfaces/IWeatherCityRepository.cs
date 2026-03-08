using API.Entities;

namespace API.Interfaces;

public interface IWeatherCityRepository
{
    Task SaveAsync(WeatherCity entity);
}
