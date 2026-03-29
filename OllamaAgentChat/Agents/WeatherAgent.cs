using System.ComponentModel;

namespace OllamaAgentChat.Agents;

/// <summary>
/// Agent specializzato per informazioni meteorologiche
/// </summary>
public class WeatherAgent
{
    [Description("Get the weather for a given location.")]
    public static string GetWeather([Description("The location to get the weather for.")] string location)
        => $"The weather in {location} is cloudy with a high of 15°C.";

    [Description("Get the weather forecast for the next days.")]
    public static string GetWeatherForecast(
        [Description("The location to get the forecast for.")] string location,
        [Description("Number of days for the forecast (1-7).")] int days = 3)
    {
        return $"Weather forecast for {location} for the next {days} days:\n" +
               $"- Day 1: Sunny, 18°C\n" +
               $"- Day 2: Partly cloudy, 16°C\n" +
               $"- Day 3: Rainy, 12°C";
    }

    [Description("Get current temperature for a specific location.")]
    public static string GetTemperature([Description("The location to get temperature for.")] string location)
        => $"Current temperature in {location} is 15°C.";
}
