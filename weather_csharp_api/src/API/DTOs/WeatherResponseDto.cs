namespace API.DTOs;

public record WeatherResponseDto(
    LocationResponseDto Location,
    CurrentWeatherResponseDto Current
);

public record LocationResponseDto(
    string Name,
    string Region,
    string Country,
    double Lat,
    double Lon,
    string Timezone,
    string LocalTime
);

public record CurrentWeatherResponseDto(
    double TempC,
    double TempF,
    bool IsDay,
    ConditionResponseDto Condition,
    double WindMph,
    double WindKph,
    int WindDegree,
    string WindDir,
    double PressureMb,
    double PrecipMm,
    int Humidity,
    int Cloud,
    double FeelsLikeC,
    double FeelsLikeF,
    double Uv
);

public record ConditionResponseDto(
    string Text,
    string Icon,
    int Code
);
