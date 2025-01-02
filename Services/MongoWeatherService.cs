using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using System.Threading.Tasks;

// Service to interact with MongoDB
public class MongoWeatherService
{
    private readonly IMongoCollection<BsonDocument> _weatherCollection;
    private readonly IMongoCollection<BsonDocument> _favoritesCollection;

    // Constructor to initialize MongoDB connection and collections
    public MongoWeatherService()
    {
        var client = new MongoClient("mongodb://localhost:27017"); // MongoDB connection string
        var database = client.GetDatabase("WeatherApplicationDB");
        _weatherCollection = database.GetCollection<BsonDocument>("WeatherApplicationData");
        _favoritesCollection = database.GetCollection<BsonDocument>("FavoriteCities"); // New collection for favorites
    }

    // Method to add weather data to MongoDB
    public async Task AddWeatherData(WeatherResponse weatherData)
    {
        DateTime utcTime = DateTime.UtcNow;

        // Convert UTC time to Indian Standard Time (IST)
        TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, indiaTimeZone);

        // Format the IST time as MM/dd/yyyy HH:mm
        string timestamp = istTime.ToString("MM/dd/yyyy HH:mm");

        var document = new BsonDocument
        {
            { "City", weatherData.Name },
            { "Temperature", weatherData.Main?.Temp },
            { "Humidity", weatherData.Main?.Humidity },
            { "Pressure", weatherData.Main?.Pressure },
            { "Timestamp", timestamp }
        };

        await _weatherCollection.InsertOneAsync(document);
    }

    // Method to add a city to the favorites collection
    public async Task AddToFavorites(string cityName)
    {
        var existingCity = await _favoritesCollection.Find(new BsonDocument { { "City", cityName } }).FirstOrDefaultAsync();

        if (existingCity == null)
        {
            var favoriteCity = new BsonDocument
            {
                { "City", cityName }
            };

            await _favoritesCollection.InsertOneAsync(favoriteCity);
        }
    }

    // Method to retrieve all favorite cities from MongoDB
    public async Task<List<string>> GetFavoriteCities()
    {
        var cities = await _favoritesCollection.Find(new BsonDocument()).ToListAsync();
        return cities.Select(c => c["City"].AsString).ToList();
    }
}

// Class to store weather data fetched from Blazor App 
public class WeatherResponse
{
    public Main? Main { get; set; }
    public required string Name { get; set; }
}

// Class to store main weather data (Temperature, Pressure, Humidity)
public class Main
{
    public float? Temp { get; set; }
    public float? Pressure { get; set; }
    public float? Humidity { get; set; }
}
