using Polly.CircuitBreaker;
using Polly.Timeout;
using Polly;
using SpaceWeatherStation.Interfaces;
using Microsoft.Data.SqlClient;
using SpaceWeatherStation.Entities;
using Polly.Wrap;
using System.ComponentModel;

namespace SpaceWeatherStation.Services
{
    public class ApplicationDataService:IApplicationDataService
    {
        private readonly IDatabaseRepository _databaseRepository;
        private readonly ICacheManager _cacheManager;
        private readonly ICircuitBreakerFactory _circuitBreakerFactory;
        public ApplicationDataService(IDatabaseRepository databaseRepository, ICacheManager cacheManager, ICircuitBreakerFactory circuitBreakerFactory)
        {
            _databaseRepository = databaseRepository;
            _cacheManager = cacheManager;
            _circuitBreakerFactory = circuitBreakerFactory;
        }

        private readonly IAsyncPolicy _getRetryPolicy = Policy.Handle<SqlException>()
                            .Or<Win32Exception>()
                            .Or<TimeoutRejectedException>()
                            .Or<TaskCanceledException>()
                            .WaitAndRetryAsync(8, retryAttempt => TimeSpan.FromMilliseconds(300));

        private readonly IAsyncPolicy _InsertRetryPolicy = Policy.Handle<SqlException>()
                            .Or<TimeoutRejectedException>()
                            .Or<TaskCanceledException>()
                            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        private readonly IAsyncPolicy _getTimeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromMilliseconds(1500), TimeoutStrategy.Pessimistic);

        private readonly IAsyncPolicy _insertTimeoutPolicy = Policy.TimeoutAsync(60, TimeoutStrategy.Pessimistic);

        public async Task<string> GetLastWeatherDataFromDatabase()
        {
            var circuitBreakerPolicy= _circuitBreakerFactory.GetDBCircuitBreakerPolicy();
            var fullPolicy = Policy.WrapAsync(_getTimeoutPolicy, circuitBreakerPolicy, _getRetryPolicy);

            if (circuitBreakerPolicy.CircuitState == CircuitState.Open)
            {
                return null;
            }

            try
            {
                var ress = await fullPolicy.ExecuteAsync(async () =>
                {
                    var r = await _databaseRepository.GetLastWeatherData();
                    return r;
                });
                return ress;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> InsertWeatherData(APIWeatherData aPIWeatherData)
        {
            var circuitBreakerPolicy = _circuitBreakerFactory.GetDBCircuitBreakerPolicy();
            var fullPolicy = Policy.WrapAsync(_insertTimeoutPolicy, circuitBreakerPolicy, _InsertRetryPolicy);

            if (circuitBreakerPolicy.CircuitState == CircuitState.Open)
            {
                return false;
            }

            try
            {
                var ress = await fullPolicy.ExecuteAsync(async () =>
                    await _databaseRepository.InsertWeatherData(aPIWeatherData));
                return ress;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task ArchiveOldData()
        {
            var circuitBreakerPolicy = _circuitBreakerFactory.GetDBCircuitBreakerPolicy();

            if (circuitBreakerPolicy.CircuitState == CircuitState.Open)
            {
                return;
            }

            await _databaseRepository.ArchiveOldData();
        }

        public async Task<string> GetLastWeatherDataFromCache()
        {
            var weatherData= await _cacheManager.GetCacheAsync<string>("LastWeatherData");

            return weatherData;
        }
    }
}
