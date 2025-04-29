using AnalysisService.Cache;
using AnalysisService.Models;
using AnalysisService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AnalysisService.Consumers
{
    public class PlantDataConsumer
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly ILogger<PlantDataConsumer> _logger;
        private readonly IRedisBufferService _redisBufferService;
        private readonly IAIAnalysisService _analysisService;

        public PlantDataConsumer(IRabbitMQService rabbitMQService, ILogger<PlantDataConsumer> logger, IRedisBufferService redisBufferService, IAIAnalysisService analysisService)
        {
            _rabbitMQService = rabbitMQService;
            _logger = logger;
            _redisBufferService = redisBufferService;
            _analysisService = analysisService;
        }

        public async Task StartConsumingAsync()
        {
            await _rabbitMQService.InitializeAsync();

            var channel = _rabbitMQService.GetChannel();

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation($"Получены данные от plants: {message}");

                var plantData = JsonSerializer.Deserialize<PlantDataDto>(message);
                var userId = plantData.UserId;

                var buffer = await _redisBufferService.GetRequestAsync(userId) ?? new AnalysisRequest();
                buffer.PlantData = plantData;

                if (!string.IsNullOrWhiteSpace(buffer.WeatherJson))
                {
                    var result = await _analysisService.AnalyzeAsync(buffer);
                    _logger.LogInformation($"Результат анализа: {result.Recommendation}");
                    await _redisBufferService.DeleteRequestAsync(userId);
                }
                else
                {
                    await _redisBufferService.SaveRequestAsync(userId, buffer);
                }
            };

            // Объявляем очередь с параметрами:
            // - очередь долговечная (не удаляется при перезапуске RabbitMQ)
            // - не эксклюзивная (может использоваться несколькими потребителями)
            // - не автозакрывающаяся (не удаляется, если никто не подключается)
            await channel.QueueDeclareAsync(
                queue: "plants",  // Имя очереди
                durable: true,  // Очередь долговечна
                exclusive: false,  // Очередь не эксклюзивна
                autoDelete: false,  // Очередь не удаляется автоматически
                arguments: null  // Дополнительные параметры (пусто)
                                );

            await channel.BasicConsumeAsync(
                queue: "plants",
                autoAck: true,
                consumer: consumer
);

            _logger.LogInformation("PlantDataConsumer начал потребление из очереди plants.");
        }
    }
}
