namespace AnalysisService.Consumers
{
    public class InitRabbitMQService : IHostedService
    {
        private readonly IRabbitMQService _service;
        private readonly ILogger<InitRabbitMQService> _logger;

        public InitRabbitMQService(IRabbitMQService service, ILogger<InitRabbitMQService> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _service.InitializeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Ошибка инициализации RabbitMQ");
                throw; // Приложение не запустится
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
