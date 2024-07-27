namespace SpaceWeatherStation.Entities
{
    public class APIWeatherData
    {
        public int Id { get; set; }
        public string JsonValue { get; set; }
        public DateTime APIResponseDate { get; set; }
    }
}
