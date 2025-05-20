namespace AnalysisService.Models
{
    public class AnalysisRequest
    {
        public PlantDataDto PlantData { get; set; } = new();
        public string WeatherJson { get; set; } = string.Empty;
    }
}
