namespace DataInputService.Domain.Entities
{
    public class Plant
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string PlantType { get; set; }
        public float? SoilTemperature { get; set; }
        public float? SoilHumidity { get; set; }
        public float? SoilPh { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
