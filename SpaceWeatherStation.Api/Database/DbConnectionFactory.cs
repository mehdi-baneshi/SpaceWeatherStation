using Microsoft.AspNetCore.Connections;
using SpaceWeatherStation.Interfaces;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SpaceWeatherStation.Database
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        private readonly string _masterConnectionString;

        public DbConnectionFactory(string connectionString, string masterConnectionString)
        {
            _connectionString = connectionString;
            _masterConnectionString = masterConnectionString;
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<IDbConnection> CreateMasterConnectionAsync()
        {
            var connection = new SqlConnection(_masterConnectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}