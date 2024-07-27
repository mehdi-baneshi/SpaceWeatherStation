using System.Data;

namespace SpaceWeatherStation.Interfaces
{
    public interface IDbConnectionFactory
    {
        public Task<IDbConnection> CreateConnectionAsync();
        public Task<IDbConnection> CreateMasterConnectionAsync();
    }
}
