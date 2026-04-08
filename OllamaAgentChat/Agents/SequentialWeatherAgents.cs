using System.ComponentModel;

namespace OllamaAgentChat.Agents;

/// <summary>
/// Agent specializzato per analisi condizioni meteorologiche
/// </summary>
public class WeatherAnalyzerAgent
{
    [Description("Analyze current weather conditions and provide detailed insights.")]
    public static string AnalyzeWeatherConditions(
        [Description("The location to analyze weather for.")] string location)
    {
        return $"Analyzing weather conditions for {location}:\n" +
               $"- Current conditions: Partly cloudy\n" +
               $"- Humidity: 65%\n" +
               $"- Wind: 15 km/h NW\n" +
               $"- Pressure: 1013 hPa\n" +
               $"- Visibility: 10 km";
    }

    [Description("Get UV index and sun protection recommendations.")]
    public static string GetUVIndex([Description("The location.")] string location)
    {
        return $"UV Index for {location}: 6 (High)\n" +
               $"Recommendations: Wear sunscreen, sunglasses, and protective clothing if outdoors for extended periods.";
    }
}

/// <summary>
/// Agent specializzato per allerte meteorologiche
/// </summary>
public class WeatherAlertAgent
{
    [Description("Check for weather alerts and warnings.")]
    public static string CheckWeatherAlerts([Description("The location to check alerts for.")] string location)
    {
        return $"Weather alerts for {location}:\n" +
               $"- No severe weather alerts at this time\n" +
               $"- Standard weather watch in effect\n" +
               $"- Next update: 2 hours";
    }

    [Description("Get storm tracking information.")]
    public static string TrackStorms([Description("The location to track storms near.")] string location)
    {
        return $"Storm tracking for {location}:\n" +
               $"- No active storms within 100km\n" +
               $"- Weather system moving east at 25 km/h\n" +
               $"- Expected arrival time: 6 hours";
    }
}

/// <summary>
/// Agent specializzato per raccomandazioni meteo
/// </summary>
public class WeatherAdvisorAgent
{
    [Description("Provide activity recommendations based on weather.")]
    public static string GetActivityRecommendations(
        [Description("The location.")] string location,
        [Description("Type of activity (outdoor, indoor, sports).")] string activityType = "outdoor")
    {
        return $"Activity recommendations for {location} ({activityType}):\n" +
               $"- Morning (6-12): Good for outdoor activities\n" +
               $"- Afternoon (12-18): Partly cloudy, suitable for most activities\n" +
               $"- Evening (18-24): Cooler, dress warmly\n" +
               $"- Best time today: 10:00-14:00";
    }

    [Description("Get clothing recommendations based on weather.")]
    public static string GetClothingAdvice([Description("The location.")] string location)
    {
        return $"Clothing advice for {location}:\n" +
               $"- Light jacket or sweater recommended\n" +
               $"- Comfortable layers for temperature changes\n" +
               $"- Umbrella might be useful\n" +
               $"- Sunglasses for UV protection";
    }
}

/// <summary>
/// Agent specializzato per dati storici e statistiche meteo
/// </summary>
public class WeatherStatisticsAgent
{
    [Description("Get historical weather data and statistics.")]
    public static string GetHistoricalWeather(
        [Description("The location.")] string location,
        [Description("Number of days to look back.")] int daysBack = 7)
    {
        return $"Historical weather for {location} (last {daysBack} days):\n" +
               $"- Average temperature: 14°C\n" +
               $"- Highest: 19°C (3 days ago)\n" +
               $"- Lowest: 9°C (5 days ago)\n" +
               $"- Total precipitation: 15mm\n" +
               $"- Sunny days: 4 of {daysBack}";
    }

    [Description("Compare current weather with seasonal averages.")]
    public static string CompareWithSeasonalAverage([Description("The location.")] string location)
    {
        return $"Weather comparison for {location}:\n" +
               $"- Current temp: 15°C vs Seasonal avg: 13°C (+2°C)\n" +
               $"- Current conditions: Normal for this time of year\n" +
               $"- Precipitation: Below average\n" +
               $"- Overall: Slightly warmer than usual";
    }
}
