using Dapper;
using SpaceWeatherStation.Entities;
using SpaceWeatherStation.Interfaces;
using System.Data;
using System.Data.Common;

namespace SpaceWeatherStation.Database
{
    public class DatabaseRepository: IDatabaseRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DatabaseRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<string> GetLastWeatherData()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            string sqlQuery = @"
                SELECT TOP 1 * 
                FROM APIWeatherData WITH (nolock)
                ORDER BY APIResponseDate DESC";
            var weatherData= await connection.QuerySingleOrDefaultAsync<APIWeatherData>(sqlQuery, commandTimeout: 1);

            return weatherData?.JsonValue;
        }

        public async Task<bool> InsertWeatherData(APIWeatherData aPIWeatherData)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            string sqlQuery = @"
                INSERT INTO APIWeatherData (JsonValue, APIResponseDate)
                VALUES (@JsonValue, @APIResponseDate);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var newId = await connection.QuerySingleAsync<int>(sqlQuery, new
            {
                aPIWeatherData.JsonValue,
                aPIWeatherData.APIResponseDate
            });

            return newId>0;
        }

        public async Task ArchiveOldData()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            string query = @"
                BEGIN TRANSACTION;

                BEGIN TRY

                    IF OBJECT_ID('tempdb..#RecordsToMove') IS NOT NULL
                        DROP TABLE #RecordsToMove;

                    SELECT 
                        Id, JsonValue, APIResponseDate,
                        ROW_NUMBER() OVER (ORDER BY APIResponseDate DESC) AS RowNum
                    INTO #RecordsToMove
                    FROM 
                        APIWeatherData;

                    INSERT INTO APIWeatherDataArchive (JsonValue, APIResponseDate)
                    SELECT JsonValue, APIResponseDate
                    FROM #RecordsToMove
                    WHERE RowNum > 100;

                    DELETE FROM APIWeatherData
                    WHERE Id IN (SELECT Id FROM #RecordsToMove WHERE RowNum > 100);

                    DROP TABLE #RecordsToMove;

                    COMMIT;
                END TRY
                BEGIN CATCH
                    ROLLBACK;
                END CATCH;
            ";

            await connection.ExecuteAsync(query);
        }
    }
}
