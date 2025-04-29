using AnalysisService.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace AnalysisService.Services
{
    public class AIAnalysisService : IAIAnalysisService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AIAnalysisService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AnalysisResultDto> AnalyzeAsync(AnalysisRequest request)
        {
            var prompt = $"Ты агроном. Проанализируй растение типа {request.PlantData.PlantType}.\n" +
                         $"Температура почвы: {request.PlantData.SoilTemperature}°C\n" +
                         $"Влажность почвы: {request.PlantData.SoilHumidity}%\n" +
                         $"Кислотность почвы: {request.PlantData.SoilPh}\n" +
                         $"Погодные данные (JSON): {request.WeatherJson}\n" +
                         $"Дай рекомендации: нужно ли поливать, удобрять, какие риски? и не только по этому растению но и вцелом что может быть полезно дачнику или огороднику";

            var requestBody = new
            {
                model = "deepseek/deepseek-chat-v3-0324",
                messages = new[]
                {
                new { role = "system", content = "Ты помощник-агроном, анализирующий состояние растений и погодные условия." },
                new { role = "user", content = prompt }
            }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["OpenRouter:ApiKey"]);
            requestMessage.Content = content;

            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"AI Error: {response.StatusCode}\n{error}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);

            var resultText = doc.RootElement
                                .GetProperty("choices")[0]
                                .GetProperty("message")
                                .GetProperty("content")
                                .GetString();

            return new AnalysisResultDto
            {
                Recommendation = resultText?.Trim() ?? "Нет рекомендации от ИИ",
                NeedsWatering = resultText?.ToLower().Contains("полив") ?? false,
                NeedsFertilizing = resultText?.ToLower().Contains("удобр") ?? false,
                RiskLevel = resultText?.ToLower().Contains("риск") ?? false ? "Есть риск" : "Низкий риск"
            };
        }
    }
}
