using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiLingualWASMLab.DTO;
using System.Net.Http.Headers;

namespace MultiLingualWASMLab.Server.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class WeatherForecastController : ControllerBase
  {
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
      _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public List<WeatherForecast> Get([FromHeader] string authorization)
    {
      // 驗證 token 有在變化
      if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
      {
        // we have a valid AuthenticationHeaderValue that has the following details:
        // scheme will be "Bearer"
        string scheme = headerValue.Scheme;
        // parmameter will be the token itself.
        string? token = headerValue.Parameter;
      }

      return Enumerable.Range(1, 5).Select(index => new WeatherForecast
      {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
      })
      .ToList();
    }
  }
}