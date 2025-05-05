using AnalysisService.Cache;
using AnalysisService.Models;
using AnalysisService.Services;
using MailKit.Net.Smtp;
using MimeKit;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace AnalysisService.Consumers
{
    public class WeatherDataConsumer
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly ILogger<WeatherDataConsumer> _logger;
        private readonly IRedisBufferService _redisBufferService;
        private readonly IAIAnalysisService _analysisService;

        public WeatherDataConsumer(IRabbitMQService rabbitMQService, ILogger<WeatherDataConsumer> logger, IRedisBufferService redisBufferService,
        IAIAnalysisService analysisService)
        {
            _rabbitMQService = rabbitMQService;
            _logger = logger;
            _redisBufferService = redisBufferService;
            _analysisService = analysisService;
        }

        public async Task StartConsumingAsync()
        {
            // Подключение к RabbitMQ (создание подключения и канала, если они ещё не были созданы)
            await _rabbitMQService.InitializeAsync();

            // Получение канала (через который происходит взаимодействие с очередью RabbitMQ)
            var channel = _rabbitMQService.GetChannel();

            // Создаём асинхронного потребителя для обработки сообщений
            var consumer = new AsyncEventingBasicConsumer(channel);

            // Указываем обработчик события "сообщение получено"
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    // Получаем тело сообщения (массив байтов) и конвертируем его в строку
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation($"Получены погодные данные: {message}");

                    // Парсим строку как JSON-документ
                    using var doc = JsonDocument.Parse(message);
                    var root = doc.RootElement;

                    // Извлекаем userId и само тело прогноза погоды (в виде JSON-строки)
                    var userId = root.GetProperty("UserId").GetString();
                    var weatherJson = root.GetProperty("WeatherJson").GetRawText(); // сохраняем вложенный JSON как строку

                    // Если userId отсутствует — логируем предупреждение и пропускаем сообщение
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        _logger.LogWarning("userId отсутствует в сообщении.");
                        return;
                    }

                    // Пытаемся получить буфер анализа из Redis по userId (если его нет — создаём новый)
                    var buffer = await _redisBufferService.GetRequestAsync(userId) ?? new AnalysisRequest();

                    // Сохраняем полученные погодные данные в буфер
                    buffer.WeatherJson = weatherJson;

                    // Если данные о растении уже получены — запускаем анализ
                    if (buffer.PlantData is not null && !string.IsNullOrWhiteSpace(buffer.PlantData.PlantType))
                    {
                        // Запускаем анализ и логируем результат
                        var result = await _analysisService.AnalyzeAsync(buffer);
                        _logger.LogInformation($"Результат анализа для userId {userId}: {result.Recommendation}");

                        // ✅ Отправляем email
                        var email = "denisgavrilkov1@gmail.com"; //  Gmail

                        var mes = new MimeMessage();
                        mes.From.Add(MailboxAddress.Parse(email));
                        mes.To.Add(MailboxAddress.Parse(email)); // можно отправить самому себе
                        mes.Subject = $"Рекомендации от GardenHelper";

                        mes.Body = new TextPart("plain")
                        {
                            Text = $"{result.Recommendation}" +
                        $"{result.NeedsFertilizing}" +
                        $"{result.NeedsWatering}" +
                        $"{result.RiskLevel}"
                        };

                        using var client = new SmtpClient();
                        await client.ConnectAsync("smtp.gmail.com", 587, false);
                        await client.AuthenticateAsync(email, "ryni tkzy ikbo ddqh"); // сюда вставь Gmail App Password
                        await client.SendAsync(mes);
                        await client.DisconnectAsync(true);

                        // Удаляем буфер из Redis, так как анализ завершён
                        await _redisBufferService.DeleteRequestAsync(userId);
                    }
                    else
                    {
                        // Если данных о растении ещё нет — сохраняем буфер в Redis до получения второго пакета
                        await _redisBufferService.SaveRequestAsync(userId, buffer);
                        _logger.LogInformation("Сохранены погодные данные в Redis до поступления данных о растении.");
                    }
                }
                catch (Exception ex)
                {
                    // Логируем любую ошибку, произошедшую при обработке
                    _logger.LogError(ex, "Ошибка обработки погодных данных.");
                }
            };

            // Объявляем очередь, если она ещё не существует
            await channel.QueueDeclareAsync(
                queue: "weatherContents",    // Имя очереди
                durable: true,               // Очередь сохраняется при перезапуске брокера
                exclusive: false,            // Очередь доступна другим соединениям
                autoDelete: false,           // Очередь не удаляется автоматически при отсутствии потребителей
                arguments: null              // Нет дополнительных аргументов
            );

            // Подписываемся на очередь для начала получения сообщений
            await channel.BasicConsumeAsync(
                queue: "weatherContents",    // Очередь, из которой читаем сообщения
                autoAck: true,               // Включено автоматическое подтверждение (сообщение удаляется сразу после доставки)
                consumer: consumer           // Указанный выше потребитель обрабатывает сообщения
            );

            _logger.LogInformation("WeatherDataConsumer начал потребление из очереди weatherContents.");
        }
    }
}
