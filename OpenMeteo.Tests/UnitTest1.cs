namespace OpenMeteo.Tests;
using OpenMeteo;

public class UnitTest1
{
    [Fact]
    public async void Test1()
    {
        var client = new OpenMeteoClient();
        var uri = new Uri("https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&hourly=temperature_2m,windspeed_10m&daily=temperature_2m_max,temperature_2m_min&format=flatbuffers&timezone=auto");
        var results = await client.GetWeather(uri);

        var result = results[0];
        Console.WriteLine($"Latitude: {result.Latitude}");
        Console.WriteLine($"Longitude: {result.Longitude}");
        Console.WriteLine($"Elevation: {result.Elevation}");
        Console.WriteLine($"Timezone: {result.Timezone} (UTC offset {result.UtcOffsetSeconds} seconds)");

        var hourly = result.Hourly ?? throw new ArgumentNullException(nameof(result.Hourly));
        var temperature_2m = hourly.Variables(0)?.GetValuesArray() ?? throw new ArgumentNullException("temperature_2m");
        var windspeed_10m = hourly.Variables(1)?.GetValuesArray() ?? throw new ArgumentNullException("windspeed_10m");
        Console.WriteLine();
        Console.WriteLine("hour | temperature_2m | windspeed_10m");
        for (var i = 0; i < (hourly.TimeEnd - hourly.Time) / hourly.Interval; i += 1) {
            // By adding `UtcOffsetSeconds` the print function below will print the local date
            var time = hourly.Time + i * hourly.Interval + result.UtcOffsetSeconds;
            Console.Write(DateTimeOffset.FromUnixTimeSeconds(time).ToString("yyyy-MM-dd HH:mm:ss"));
            Console.Write($" | {temperature_2m[i]}");
            Console.Write($" | {windspeed_10m[i]}");
            Console.Write("\n");
        }

        var daily = result.Daily ?? throw new ArgumentNullException(nameof(result.Daily));
        var temperature_2m_max = daily.Variables(0)?.GetValuesArray() ?? throw new ArgumentNullException("temperature_2m_max");
        var temperature_2m_min = daily.Variables(1)?.GetValuesArray() ?? throw new ArgumentNullException("temperature_2m_min");
        Console.WriteLine();
        Console.WriteLine("day | temperature_2m_max | temperature_2m_min");
        for (var i = 0; i < (daily.TimeEnd - daily.Time) / daily.Interval; i += 1) {
            // By adding `UtcOffsetSeconds` the print function below will print the local date
            var time = daily.Time + i * daily.Interval + result.UtcOffsetSeconds;
            Console.Write(DateTimeOffset.FromUnixTimeSeconds(time).ToString("yyyy-MM-dd HH:mm:ss"));
            Console.Write($" | {temperature_2m_max[i]}");
            Console.Write($" | {temperature_2m_min[i]}");
            Console.Write("\n");
        }
    }
}