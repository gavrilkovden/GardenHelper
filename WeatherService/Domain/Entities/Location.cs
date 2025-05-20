namespace WeatherService.Domain.Entities
{
    public class Location
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
