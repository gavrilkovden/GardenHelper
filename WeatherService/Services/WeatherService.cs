using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WeatherService.Domain;
using WeatherService.Domain.Entities;
using IDatabase = StackExchange.Redis.IDatabase;

namespace WeatherService.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly WeatherDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IDatabase _redisDb;
        private readonly IRabbitMqPublisher _rabbitMQPublisher;

        public WeatherService(WeatherDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration, IConnectionMultiplexer redis, IRabbitMqPublisher rabbitMQPublisher)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _redisDb = redis.GetDatabase();
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public async Task SaveOrUpdateUserLocationAsync(string userId, double latitude, double longitude)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(x => x.UserId == userId);

            if (location == null)
            {
                location = new Location { UserId = userId, Latitude = latitude, Longitude = longitude };
                _context.Locations.Add(location);
            }
            else
            {
                location.Latitude = latitude;
                location.Longitude = longitude;
                _context.Locations.Update(location);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<string> GetWeatherAsync(string userId)
        {
            // Пытаемся получить из кэша
            var cacheKey = $"weather:{userId}"; // Это создаёт строку-ключ для Redis, чтобы сохранить прогноз погоды отдельно для каждого пользователя.
            var cachedWeather = await _redisDb.StringGetAsync(cacheKey); //здесь проверяем есть ли по этому ключу уже в БД ответ

            if (cachedWeather.HasValue) // если ответ уже есть в редис то его и возвращаем
            {
                return cachedWeather;
            }

            // Если в кэше нет — идём в OpenWeather
            var location = await _context.Locations.FirstOrDefaultAsync(x => x.UserId == userId);
            if (location == null)
                throw new Exception("User location not found.");

            var apiKey = _configuration["OpenWeather:ApiKey"];
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={location.Latitude}&lon={location.Longitude}&units=metric&appid={apiKey}&lang=ru";

            var httpClient = _httpClientFactory.CreateClient();
            var content = await httpClient.GetStringAsync(url);

            var weatherMessage = new
            {
                UserId = userId,
                WeatherJson = content
            };


            // Сохраняем в кэш на 30 минут
            await _redisDb.StringSetAsync(cacheKey, content, TimeSpan.FromMinutes(60)); // здесь значение cacheKey берем из начала метода где мы его сами и создавали

            // Публикуем прогноз в RabbitMQ!
            await _rabbitMQPublisher.PublishAsync(weatherMessage);

            return content;
        }
    }
}
