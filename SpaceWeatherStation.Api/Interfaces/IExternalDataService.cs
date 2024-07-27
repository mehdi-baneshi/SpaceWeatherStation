namespace SpaceWeatherStation.Interfaces
{
    public interface IExternalDataService
    {
        public Task<string> GetForecastDataFromWebService();
    }
}
