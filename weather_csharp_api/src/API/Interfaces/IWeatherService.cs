using API.DTOs;

namespace API.Interfaces;

public interface IWeatherService
{
    Task<WeatherResponseDto> GetWeatherByCityAsync(string city);
}
