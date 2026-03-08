<weather-api>
    api key: e78********TYUYT****
</weather-api>
<request-example>
    endpoint: http://api.weatherapi.com/v1/
    response format:
        -   current.xml
        -   current.json
    parameter q: 'then name of city'
    </request-example>
    Complete request: https://api.weatherapi.com/v1/current.json?q=string&aqi=no&pollen=no&lang=string&current_fields=string
    Complete response: ```json
            {
                "location": {
                    "name": "Brasilia",
                    "region": "Distrito Federal",
                    "country": "Brazil",
                    "lat": -15.7833,
                    "lon": -47.9167,
                    "tz_id": "America/Sao_Paulo",
                    "localtime_epoch": 1772986741,
                    "localtime": "2026-03-08 13:19"
                },
                "current": {
                    "last_updated_epoch": 1772986500,
                    "last_updated": "2026-03-08 13:15",
                    "temp_c": 24.1,
                    "temp_f": 75.4,
                    "is_day": 1,
                    "condition": {
                        "text": "Moderate or heavy rain with thunder",
                        "icon": "//cdn.weatherapi.com/weather/64x64/day/389.png",
                        "code": 1276
                    },
                    "wind_mph": 6.5,
                    "wind_kph": 10.4,
                    "wind_degree": 34,
                    "wind_dir": "NE",
                    "pressure_mb": 1017.0,
                    "pressure_in": 30.03,
                    "precip_mm": 0.01,
                    "precip_in": 0.0,
                    "humidity": 83,
                    "cloud": 75,
                    "feelslike_c": 25.4,
                    "feelslike_f": 77.6,
                    "windchill_c": 27.2,
                    "windchill_f": 81.0,
                    "heatindex_c": 28.1,
                    "heatindex_f": 82.5,
                    "dewpoint_c": 16.8,
                    "dewpoint_f": 62.3,
                    "vis_km": 8.0,
                    "vis_miles": 4.0,
                    "uv": 12.7,
                    "gust_mph": 7.5,
                    "gust_kph": 12.0,
                    "short_rad": 0.0,
                    "diff_rad": 0.0,
                    "dni": 0.0,
                    "gti": 0.0
                }
            }
    ```