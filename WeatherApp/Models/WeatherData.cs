namespace WeatherApp.Models
{
    /// <summary>
    /// Represents comprehensive weather data for a location
    /// </summary>
    public class WeatherData
    {
        public string? LocationName { get; set; }
        public string? Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public CurrentWeather? Current { get; set; }
        public List<HourlyForecast>? HourlyForecasts { get; set; }
        public List<DailyForecast>? DailyForecasts { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Current weather conditions
    /// </summary>
    public class CurrentWeather
    {
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public int WindDegree { get; set; }
        public string? WindDirection { get; set; }
        public double Pressure { get; set; }
        public double UVIndex { get; set; }
        public int CloudCover { get; set; }
        public double Visibility { get; set; }
        public string? Condition { get; set; }
        public string? ConditionIcon { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
    }

    /// <summary>
    /// Hourly weather forecast
    /// </summary>
    public class HourlyForecast
    {
        public DateTime DateTime { get; set; }
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public string? Condition { get; set; }
        public string? ConditionIcon { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public int ChanceOfRain { get; set; }
        public double PrecipitationAmount { get; set; }
    }

    /// <summary>
    /// Daily weather forecast
    /// </summary>
    public class DailyForecast
    {
        public DateTime Date { get; set; }
        public double MaxTemperature { get; set; }
        public double MinTemperature { get; set; }
        public double AvgTemperature { get; set; }
        public string? Condition { get; set; }
        public string? ConditionIcon { get; set; }
        public int ChanceOfRain { get; set; }
        public double TotalPrecipitation { get; set; }
        public double MaxWindSpeed { get; set; }
        public double AvgHumidity { get; set; }
        public double UVIndex { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
    }
}
