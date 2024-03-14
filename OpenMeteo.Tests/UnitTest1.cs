namespace OpenMeteo.Tests;
using OpenMeteo;

public class UnitTest1
{
    [Fact]
    public async void Test1()
    {
        var client = new OpenMeteoClient();
        var uri = new Uri("https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&hourly=temperature_2m&format=flatbuffers");
        var results = await client.GetWeather(uri);

        Console.WriteLine(results[0].Latitude);
    }
}