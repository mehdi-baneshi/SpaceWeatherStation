namespace SpaceWeatherStation.Entities
{
    public class APIWeatherDataArchive
    {
        public int Id { get; set; }
        public string JsonValue { get; set; }
        public DateTime APIResponseDate { get; set; }
    }
}
