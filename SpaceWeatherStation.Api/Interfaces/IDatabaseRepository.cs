using SpaceWeatherStation.Entities;

namespace SpaceWeatherStation.Interfaces
{
    public interface IDatabaseRepository
    {
        public Task<string> GetLastWeatherData();
        public Task<bool> InsertWeatherData(APIWeatherData aPIWeatherData);
        public Task ArchiveOldData();
    }
}
