using Dapper;
using Microsoft.Data.SqlClient;
using SpaceWeatherStation.Interfaces;

namespace SpaceWeatherStation.Database
{
    public class DatabaseInitializer
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DatabaseInitializer(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InitializeAsync()
        {
            //using var connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB; Database=master;User id=sa;Password=p@ssWord1234;Trusted_Connection=SSPI;Encrypt=false;TrustServerCertificate=true");
            using var masterConnection =await _connectionFactory.CreateMasterConnectionAsync();
            await masterConnection.ExecuteAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SpaceWeatherStation')
                BEGIN
                    CREATE DATABASE SpaceWeatherStation;
                END");

            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'APIWeatherData')
                BEGIN
                    CREATE TABLE APIWeatherData (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        JsonValue NVARCHAR(MAX) NOT NULL,
                        APIResponseDate DATETIME NOT NULL
                    );

                    CREATE INDEX IX_APIWeatherData_APIResponseDate
                    ON APIWeatherData(APIResponseDate);
                END

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'APIWeatherDataArchive')
                BEGIN
                    CREATE TABLE APIWeatherDataArchive (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        JsonValue NVARCHAR(MAX) NOT NULL,
                        APIResponseDate DATETIME NOT NULL
                    );
                END");
        }
    }
}
