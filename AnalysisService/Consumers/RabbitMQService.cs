using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;

namespace AnalysisService.Consumers
{
    public class RabbitMQService : IDisposable, IRabbitMQService
    {
        private IConnection _connection;  // Подключение к серверу RabbitMQ
        private IChannel _channel;  // Канал для отправки и получения сообщений
        private readonly ILogger<RabbitMQService> _logger;  // Логгер для записи событий
        private readonly string _hostName;  // Имя хоста, на котором работает RabbitMQ
        private bool _isInitialized;  // Флаг, указывающий, что RabbitMQ успешно инициализирован


        public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
        {
            _hostName = configuration["RabbitMQ:HostName"];
            _logger = logger;
        }


        // Асинхронный метод инициализации подключения к RabbitMQ
        public async Task InitializeAsync()
        {
            if (_isInitialized) return;  // Если RabbitMQ уже инициализирован, ничего не делаем

            try
            {
                // Создаем фабрику для подключения к RabbitMQ
                var factory = new ConnectionFactory { HostName = _hostName };

                // Создаем асинхронное подключение
                _connection = await factory.CreateConnectionAsync();

                // Создаем асинхронный канал для общения с RabbitMQ
                _channel = await _connection.CreateChannelAsync();

                _isInitialized = true;  // Устанавливаем флаг инициализации в true
                _logger.LogInformation("RabbitMQ подключён успешно");  // Логируем успешное подключение
            }
            catch (Exception ex)
            {
                // Если возникла ошибка, логируем её и выбрасываем исключение
                _logger.LogError($"Ошибка инициализации RabbitMQ: {ex.Message}");
                throw;
            }
        }

        // Метод для получения канала RabbitMQ
        public IChannel GetChannel()
        {
            // Проверяем, что сервис был инициализирован, иначе выбрасываем исключение
            if (!_isInitialized)
                throw new InvalidOperationException("Сервис не инициализирован. Вызовите InitializeAsync()");

            // Возвращаем канал
            return _channel;
        }

        // Метод для корректного закрытия подключения и канала RabbitMQ
        public void Dispose()
        {
            // Логируем процесс закрытия подключения
            _logger.LogInformation("Закрытие подключения RabbitMQ...");

            // Освобождаем ресурсы, если канал и подключение существуют
            _channel?.Dispose();  // Закрываем канал
            _connection?.Dispose();  // Закрываем подключение

            // Останавливаем сборщик мусора от вызова финализаторов для этого объекта
            GC.SuppressFinalize(this);
        }
    }


}
