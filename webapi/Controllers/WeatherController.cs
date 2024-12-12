using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// Controller for managing weather data
[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly MongoWeatherService _mongoWeatherService;

    // Inject MongoWeatherService through the constructor
    public WeatherController(MongoWeatherService mongoWeatherService)
    {
        _mongoWeatherService = mongoWeatherService;
    }

    // POST endpoint to add weather data
    [HttpPost("add")]
    public async Task<IActionResult> AddWeatherData([FromBody] WeatherResponse weatherData)
    {
        if (weatherData == null)
        {
            return BadRequest("Invalid weather data");
        }

        await _mongoWeatherService.AddWeatherData(weatherData);
        return Ok("Weather data added successfully");
    }

    // POST endpoint to add a city to the favorites collection
    [HttpPost("favorites")]
    public async Task<IActionResult> AddToFavorites([FromBody] string cityName)
    {
        if (string.IsNullOrEmpty(cityName))
        {
            return BadRequest("City name is required.");
        }

        await _mongoWeatherService.AddToFavorites(cityName);
        return Ok("City added to favorites.");
    }

    // GET endpoint to retrieve favorite cities
    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavoriteCities()
    {
        var favoriteCities = await _mongoWeatherService.GetFavoriteCities();
        return Ok(favoriteCities);
    }
}
