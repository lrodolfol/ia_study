using System.Text.Json.Serialization;

namespace API.DTOs;

public record WeatherApiResponseDto(
    [property: JsonPropertyName("location")] WeatherApiLocationDto Location,
    [property: JsonPropertyName("current")] WeatherApiCurrentDto Current
);

public record WeatherApiLocationDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("region")] string Region,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("lat")] double Lat,
    [property: JsonPropertyName("lon")] double Lon,
    [property: JsonPropertyName("tz_id")] string TzId,
    [property: JsonPropertyName("localtime")] string LocalTime
);

public record WeatherApiCurrentDto(
    [property: JsonPropertyName("temp_c")] double TempC,
    [property: JsonPropertyName("temp_f")] double TempF,
    [property: JsonPropertyName("is_day")] int IsDay,
    [property: JsonPropertyName("condition")] WeatherApiConditionDto Condition,
    [property: JsonPropertyName("wind_mph")] double WindMph,
    [property: JsonPropertyName("wind_kph")] double WindKph,
    [property: JsonPropertyName("wind_degree")] int WindDegree,
    [property: JsonPropertyName("wind_dir")] string WindDir,
    [property: JsonPropertyName("pressure_mb")] double PressureMb,
    [property: JsonPropertyName("precip_mm")] double PrecipMm,
    [property: JsonPropertyName("humidity")] int Humidity,
    [property: JsonPropertyName("cloud")] int Cloud,
    [property: JsonPropertyName("feelslike_c")] double FeelsLikeC,
    [property: JsonPropertyName("feelslike_f")] double FeelsLikeF,
    [property: JsonPropertyName("uv")] double Uv
);

public record WeatherApiConditionDto(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("icon")] string Icon,
    [property: JsonPropertyName("code")] int Code
);
