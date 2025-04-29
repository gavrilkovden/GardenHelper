namespace AnalysisService.Models
{
    public class AnalysisResultDto
    {
        public string Recommendation { get; set; } = string.Empty;
        public bool NeedsWatering { get; set; }
        public bool NeedsFertilizing { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }
}
