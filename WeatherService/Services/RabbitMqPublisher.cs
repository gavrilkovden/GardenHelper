using Microsoft.AspNetCore.Connections;
using System.Text.Json;
using System.Text;
using RabbitMQ.Client;

namespace WeatherService.Services
{
    public class RabbitMqPublisher : IAsyncDisposable, IRabbitMqPublisher
    {
        private readonly ILogger<RabbitMqPublisher> _logger;
        private IConnection? _connection;
        private IChannel? _channel;
        private bool _isInitialized;

        // Конструктор теперь принимает только ILogger
        public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger)
        {
            _logger = logger;
        }

        public async Task InitializeAsync(string hostName)
        {
            if (_isInitialized)
            {
                _logger.LogInformation("RabbitMQ уже инициализирован.");
                return;
            }

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "rabbitmq",  // Имя сервиса из docker-compose
                    Port = 5672,
                    UserName = "guest",
                    Password = "guest"
                };

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(
                    queue: "weatherContents",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _isInitialized = true;
                _logger.LogInformation("RabbitMQ подключен успешно.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка инициализации RabbitMQ.");
                throw;
            }
        }

        public async Task PublishAsync<T>(T message)
        {
            if (_channel?.IsClosed ?? true)
                throw new InvalidOperationException("Канал не инициализирован.");

            try
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                await _channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: typeof(T).Name, // или строгое имя очереди
                    body: body
                );

                _logger.LogInformation("Успешно опубликовано сообщение типа: {Type}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка публикации сообщения типа: {Type}", typeof(T).Name);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                _logger.LogInformation("Канал RabbitMQ закрыт.");
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                _logger.LogInformation("Подключение RabbitMQ закрыто.");
            }

            _isInitialized = false;
        }
    }
}
