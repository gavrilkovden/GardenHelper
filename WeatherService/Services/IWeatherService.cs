namespace WeatherService.Services
{
    public interface IWeatherService
    {
        Task SaveOrUpdateUserLocationAsync(string userId, double latitude, double longitude);
        Task<string> GetWeatherAsync(string userId);
    }
}
