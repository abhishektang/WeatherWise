using System.Net.Http.Json;
using System.Text.Json;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    /// <summary>
    /// Service for retrieving weather data from Open-Meteo API
    /// Free weather API with no authentication required
    /// </summary>
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<WeatherData?> GetWeatherAsync(double latitude, double longitude)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetWeatherAsync called for coordinates: ({latitude}, {longitude})");
                
                // Build Open-Meteo API URL
                var url = $"https://api.open-meteo.com/v1/forecast?" +
                    $"latitude={latitude}&longitude={longitude}" +
                    $"&current=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation,weather_code,cloud_cover,pressure_msl,wind_speed_10m,wind_direction_10m" +
                    $"&hourly=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation,precipitation_probability,weather_code,wind_speed_10m" +
                    $"&daily=temperature_2m_max,temperature_2m_min,precipitation_sum,precipitation_probability_max,weather_code,wind_speed_10m_max,sunrise,sunset" +
                    $"&timezone=auto";

                System.Diagnostics.Debug.WriteLine($"Fetching weather from Open-Meteo: {url}");
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Response received, length: {json.Length}");
                
                var result = await ParseOpenMeteoData(json, latitude, longitude);
                System.Diagnostics.Debug.WriteLine($"Weather data parsed successfully: {result?.LocationName}");
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching weather: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<WeatherData?> GetWeatherByLocationAsync(string locationName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetWeatherByLocationAsync called with: {locationName}");
                
                var locations = await SearchLocationsAsync(locationName);
                
                System.Diagnostics.Debug.WriteLine($"SearchLocationsAsync returned {locations?.Count ?? 0} locations");
                
                if (locations == null || !locations.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No locations found - returning null");
                    return null;
                }

                var location = locations.First();
                System.Diagnostics.Debug.WriteLine($"Using first location: {location.Name} at ({location.Latitude}, {location.Longitude})");
                
                var weatherData = await GetWeatherAsync(location.Latitude, location.Longitude);
                
                if (weatherData != null)
                {
                    // Update location name with the searched city name
                    weatherData.LocationName = location.Name ?? locationName;
                    weatherData.Country = location.Country ?? "";
                }
                
                return weatherData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching weather by location: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<List<Models.Location>?> SearchLocationsAsync(string query)
        {
            try
            {
                // Use Open-Meteo's free geocoding API
                var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(query)}&count=5&language=en&format=json";
                System.Diagnostics.Debug.WriteLine($"Search URL: {url}");
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Search API Error: {response.StatusCode} - {errorContent}");
                    return new List<Models.Location>();
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Search Response: {json.Substring(0, Math.Min(500, json.Length))}");
                
                var data = JsonSerializer.Deserialize<JsonElement>(json);
                var locations = new List<Models.Location>();
                
                if (data.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in results.EnumerateArray())
                    {
                        var location = new Models.Location
                        {
                            Name = item.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String 
                                ? name.GetString() ?? "Unknown" : "Unknown",
                            Region = item.TryGetProperty("admin1", out var admin1) && admin1.ValueKind == JsonValueKind.String 
                                ? admin1.GetString() : null,
                            Country = item.TryGetProperty("country", out var country) && country.ValueKind == JsonValueKind.String 
                                ? country.GetString() : null,
                            Latitude = item.TryGetProperty("latitude", out var lat) ? lat.GetDouble() : 0,
                            Longitude = item.TryGetProperty("longitude", out var lon) ? lon.GetDouble() : 0
                        };
                        
                        System.Diagnostics.Debug.WriteLine($"Found location: {location.Name}, {location.Country} ({location.Latitude}, {location.Longitude})");
                        locations.Add(location);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Total locations found: {locations.Count}");
                return locations;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching locations: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<Models.Location>();
            }
        }

        private async Task<string?> GetLocationNameAsync(double latitude, double longitude)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Getting location name for: ({latitude}, {longitude})");
                return await FindNearestCityName(latitude, longitude);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting location name: {ex.Message}");
                return $"Location ({latitude:F2}, {longitude:F2})";
            }
        }

        private async Task<string> FindNearestCityName(double latitude, double longitude)
        {
            try
            {
                // Try Nominatim reverse geocoding - prioritize specific locations over broad city names
                var url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude}&lon={longitude}&format=json";
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WeatherApp/1.0");
                
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    if (data.TryGetProperty("address", out var address))
                    {
                        // Prioritize more specific locations first: suburb/neighbourhood > town/village > city
                        // This gives more accurate local names instead of broad city names
                        if (address.TryGetProperty("suburb", out var suburb) && suburb.ValueKind == JsonValueKind.String)
                        {
                            var suburbName = suburb.GetString();
                            if (!string.IsNullOrEmpty(suburbName))
                                return suburbName;
                        }
                        if (address.TryGetProperty("neighbourhood", out var neighbourhood) && neighbourhood.ValueKind == JsonValueKind.String)
                        {
                            var neighbourhoodName = neighbourhood.GetString();
                            if (!string.IsNullOrEmpty(neighbourhoodName))
                                return neighbourhoodName;
                        }
                        if (address.TryGetProperty("town", out var town) && town.ValueKind == JsonValueKind.String)
                        {
                            var townName = town.GetString();
                            if (!string.IsNullOrEmpty(townName))
                                return townName;
                        }
                        if (address.TryGetProperty("village", out var village) && village.ValueKind == JsonValueKind.String)
                        {
                            var villageName = village.GetString();
                            if (!string.IsNullOrEmpty(villageName))
                                return villageName;
                        }
                        if (address.TryGetProperty("city", out var city) && city.ValueKind == JsonValueKind.String)
                        {
                            var cityName = city.GetString();
                            if (!string.IsNullOrEmpty(cityName))
                                return cityName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Nominatim fallback failed: {ex.Message}");
            }
            
            return $"Location ({latitude:F2}, {longitude:F2})";
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Haversine formula to calculate distance between two points
            const double R = 6371; // Earth's radius in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private async Task<WeatherData> ParseOpenMeteoData(string json, double latitude, double longitude)
        {
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            
            var weatherData = new WeatherData
            {
                LocationName = await GetLocationNameAsync(latitude, longitude) ?? "Unknown Location",
                Country = "",
                Latitude = latitude,
                Longitude = longitude,
                LastUpdated = DateTime.Now
            };

            // Parse current weather
            if (data.TryGetProperty("current", out var current))
            {
                DateTime currentTime = DateTime.Now;
                if (current.TryGetProperty("time", out var time) && DateTime.TryParse(time.GetString(), out var parsedTime))
                {
                    currentTime = parsedTime;
                }

                // Get sunrise/sunset from daily
                DateTime sunrise = DateTime.Now.Date.AddHours(6);
                DateTime sunset = DateTime.Now.Date.AddHours(18);
                if (data.TryGetProperty("daily", out var daily))
                {
                    if (daily.TryGetProperty("sunrise", out var sunriseArr) && sunriseArr.ValueKind == JsonValueKind.Array && sunriseArr.GetArrayLength() > 0)
                    {
                        if (DateTime.TryParse(sunriseArr[0].GetString(), out var parsedSunrise))
                            sunrise = parsedSunrise;
                    }
                    if (daily.TryGetProperty("sunset", out var sunsetArr) && sunsetArr.ValueKind == JsonValueKind.Array && sunsetArr.GetArrayLength() > 0)
                    {
                        if (DateTime.TryParse(sunsetArr[0].GetString(), out var parsedSunset))
                            sunset = parsedSunset;
                    }
                }

                weatherData.Current = new CurrentWeather
                {
                    Temperature = current.TryGetProperty("temperature_2m", out var temp) ? temp.GetDouble() : 0,
                    FeelsLike = current.TryGetProperty("apparent_temperature", out var feels) ? feels.GetDouble() : 0,
                    Humidity = current.TryGetProperty("relative_humidity_2m", out var hum) ? hum.GetInt32() : 0,
                    Pressure = current.TryGetProperty("pressure_msl", out var press) ? press.GetDouble() : 1013,
                    WindSpeed = current.TryGetProperty("wind_speed_10m", out var wind) ? wind.GetDouble() : 0,
                    WindDegree = current.TryGetProperty("wind_direction_10m", out var windDir) ? windDir.GetInt32() : 0,
                    CloudCover = current.TryGetProperty("cloud_cover", out var clouds) ? clouds.GetInt32() : 0,
                    Visibility = 10,
                    Condition = GetWeatherCondition(current.TryGetProperty("weather_code", out var wmo) ? wmo.GetInt32() : 0),
                    ConditionIcon = GetWeatherIconFromWMO(current.TryGetProperty("weather_code", out var wmo2) ? wmo2.GetInt32() : 0),
                    DateTime = currentTime,
                    Sunrise = sunrise,
                    Sunset = sunset
                };
            }

            // Parse hourly forecasts
            weatherData.HourlyForecasts = new List<HourlyForecast>();
            if (data.TryGetProperty("hourly", out var hourly) && hourly.TryGetProperty("time", out var hourlyTimes) && hourlyTimes.ValueKind == JsonValueKind.Array)
            {
                var times = hourlyTimes.EnumerateArray().ToArray();
                var temps = hourly.TryGetProperty("temperature_2m", out var t) ? t.EnumerateArray().ToArray() : null;
                var feelsLike = hourly.TryGetProperty("apparent_temperature", out var f) ? f.EnumerateArray().ToArray() : null;
                var humidity = hourly.TryGetProperty("relative_humidity_2m", out var h) ? h.EnumerateArray().ToArray() : null;
                var windSpeed = hourly.TryGetProperty("wind_speed_10m", out var w) ? w.EnumerateArray().ToArray() : null;
                var precipProb = hourly.TryGetProperty("precipitation_probability", out var pp) ? pp.EnumerateArray().ToArray() : null;
                var precip = hourly.TryGetProperty("precipitation", out var p) ? p.EnumerateArray().ToArray() : null;
                var weatherCodes = hourly.TryGetProperty("weather_code", out var wc) ? wc.EnumerateArray().ToArray() : null;

                for (int i = 0; i < Math.Min(24, times.Length); i++)
                {
                    DateTime.TryParse(times[i].GetString(), out var hourTime);
                    var wmoCode = weatherCodes != null && i < weatherCodes.Length ? weatherCodes[i].GetInt32() : 0;

                    weatherData.HourlyForecasts.Add(new HourlyForecast
                    {
                        DateTime = hourTime,
                        Temperature = temps != null && i < temps.Length ? temps[i].GetDouble() : 0,
                        FeelsLike = feelsLike != null && i < feelsLike.Length ? feelsLike[i].GetDouble() : 0,
                        Condition = GetWeatherCondition(wmoCode),
                        ConditionIcon = GetWeatherIconFromWMO(wmoCode),
                        Humidity = humidity != null && i < humidity.Length ? humidity[i].GetInt32() : 0,
                        WindSpeed = windSpeed != null && i < windSpeed.Length ? windSpeed[i].GetDouble() : 0,
                        ChanceOfRain = precipProb != null && i < precipProb.Length ? precipProb[i].GetInt32() : 0,
                        PrecipitationAmount = precip != null && i < precip.Length ? precip[i].GetDouble() : 0
                    });
                }
            }

            // Parse daily forecasts
            weatherData.DailyForecasts = new List<DailyForecast>();
            if (data.TryGetProperty("daily", out var dailyData) && dailyData.TryGetProperty("time", out var dailyTimes) && dailyTimes.ValueKind == JsonValueKind.Array)
            {
                var times = dailyTimes.EnumerateArray().ToArray();
                var tempMax = dailyData.TryGetProperty("temperature_2m_max", out var tmax) ? tmax.EnumerateArray().ToArray() : null;
                var tempMin = dailyData.TryGetProperty("temperature_2m_min", out var tmin) ? tmin.EnumerateArray().ToArray() : null;
                var precipSum = dailyData.TryGetProperty("precipitation_sum", out var ps) ? ps.EnumerateArray().ToArray() : null;
                var precipProbMax = dailyData.TryGetProperty("precipitation_probability_max", out var ppm) ? ppm.EnumerateArray().ToArray() : null;
                var windSpeedMax = dailyData.TryGetProperty("wind_speed_10m_max", out var wsm) ? wsm.EnumerateArray().ToArray() : null;
                var weatherCodes = dailyData.TryGetProperty("weather_code", out var wcd) ? wcd.EnumerateArray().ToArray() : null;

                for (int i = 0; i < Math.Min(7, times.Length); i++)
                {
                    DateTime.TryParse(times[i].GetString(), out var dayTime);
                    var wmoCode = weatherCodes != null && i < weatherCodes.Length ? weatherCodes[i].GetInt32() : 0;
                    var maxTemp = tempMax != null && i < tempMax.Length ? tempMax[i].GetDouble() : 0;
                    var minTemp = tempMin != null && i < tempMin.Length ? tempMin[i].GetDouble() : 0;

                    weatherData.DailyForecasts.Add(new DailyForecast
                    {
                        Date = dayTime,
                        MaxTemperature = maxTemp,
                        MinTemperature = minTemp,
                        AvgTemperature = (maxTemp + minTemp) / 2,
                        Condition = GetWeatherCondition(wmoCode),
                        ConditionIcon = GetWeatherIconFromWMO(wmoCode),
                        ChanceOfRain = precipProbMax != null && i < precipProbMax.Length ? precipProbMax[i].GetInt32() : 0,
                        TotalPrecipitation = precipSum != null && i < precipSum.Length ? precipSum[i].GetDouble() : 0,
                        MaxWindSpeed = windSpeedMax != null && i < windSpeedMax.Length ? windSpeedMax[i].GetDouble() : 0,
                        AvgHumidity = 0
                    });
                }
            }

            return weatherData;
        }

        private string GetWeatherCondition(int wmoCode)
        {
            // WMO Weather interpretation codes
            return wmoCode switch
            {
                0 => "Clear sky",
                1 => "Mainly clear",
                2 => "Partly cloudy",
                3 => "Overcast",
                45 or 48 => "Foggy",
                51 or 53 or 55 => "Drizzle",
                61 or 63 or 65 => "Rain",
                71 or 73 or 75 => "Snow",
                77 => "Snow grains",
                80 or 81 or 82 => "Rain showers",
                85 or 86 => "Snow showers",
                95 => "Thunderstorm",
                96 or 99 => "Thunderstorm with hail",
                _ => "Unknown"
            };
        }

        private string GetWeatherIconFromWMO(int wmoCode)
        {
            // Map WMO codes to OpenWeatherMap-style icons
            return wmoCode switch
            {
                0 => "01d", // Clear sky
                1 => "02d", // Mainly clear
                2 => "03d", // Partly cloudy
                3 => "04d", // Overcast
                45 or 48 => "50d", // Fog
                51 or 53 or 55 => "09d", // Drizzle
                61 or 63 or 65 => "10d", // Rain
                71 or 73 or 75 or 77 => "13d", // Snow
                80 or 81 or 82 => "09d", // Rain showers
                85 or 86 => "13d", // Snow showers
                95 or 96 or 99 => "11d", // Thunderstorm
                _ => "01d"
            };
        }
    }
}
