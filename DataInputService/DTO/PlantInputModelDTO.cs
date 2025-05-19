namespace DataInputService.DTO
{
    public class PlantInputModelDTO
    {
        public string UserId { get; set; }
        public string PlantType { get; set; }
        public float SoilTemperature { get; set; }
        public float SoilHumidity { get; set; }
        public float SoilPh { get; set; }
    }
}
