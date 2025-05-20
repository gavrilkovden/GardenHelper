using RabbitMQ.Client;

namespace AnalysisService.Consumers
{
    public interface IRabbitMQService
    {
        void Dispose();
        IChannel GetChannel();
        Task InitializeAsync();
    }
}