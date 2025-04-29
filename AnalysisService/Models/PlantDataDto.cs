namespace AnalysisService.Models
{
    public class PlantDataDto
    {
        public string UserId { get; set; } = string.Empty;
        public string PlantType { get; set; } = string.Empty;
        public float SoilTemperature { get; set; }
        public float SoilHumidity { get; set; }
        public float SoilPh { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
