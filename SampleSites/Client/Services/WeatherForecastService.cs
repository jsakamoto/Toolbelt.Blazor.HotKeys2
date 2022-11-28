using System.Net.Http.Json;
using SampleSite.Components.Services;

namespace SampleSite.Client.Services;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly HttpClient HttpClient;

    public WeatherForecastService(HttpClient httpClient)
    {
        this.HttpClient = httpClient;
    }

    public Task<WeatherForecast[]?> GetForecastAsync()
    {
        return this.HttpClient.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json");
    }
}
