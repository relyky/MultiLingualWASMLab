using Refit;
using MultiLingualWASMLab.DTO;

namespace MultiLingualWASMLab.Client.RefitClient;

public interface IWeatherForecastApi
{
  [Get("/api/WeatherForecast")]
  Task<List<WeatherForecast>> WeatherForecastAsync();
}
