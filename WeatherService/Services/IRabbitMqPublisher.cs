
namespace WeatherService.Services
{
    public interface IRabbitMqPublisher
    {
        ValueTask DisposeAsync();
        Task InitializeAsync(string hostName);
        Task PublishAsync<T>(T message);
    }
}