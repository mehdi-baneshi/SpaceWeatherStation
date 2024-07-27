using SpaceWeatherStation.Entities;

namespace SpaceWeatherStation.Interfaces
{
    public interface IApplicationDataService
    {
        public Task<string> GetLastWeatherDataFromDatabase();
        public Task<bool> InsertWeatherData(APIWeatherData aPIWeatherData);
        public Task<string> GetLastWeatherDataFromCache();
        public Task ArchiveOldData();
    }
}
