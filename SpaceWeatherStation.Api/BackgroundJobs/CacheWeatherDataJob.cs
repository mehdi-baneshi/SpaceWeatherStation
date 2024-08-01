using Quartz;
using SpaceWeatherStation.Cache;
using SpaceWeatherStation.Interfaces;
using System.Runtime.InteropServices;

namespace SpaceWeatherStation.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class CacheWeatherDataJob : IJob
    {
        private readonly IExternalDataService _externalDataService;
        private readonly ICacheManager _cacheManager;

        public CacheWeatherDataJob(IExternalDataService externalDataService, ICacheManager cacheManager)
        {
            _externalDataService = externalDataService;
            _cacheManager = cacheManager;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var data = await _externalDataService.GetForecastDataFromWebService();
            if (data is not null)
            {
                await _cacheManager.RemoveCacheAsync("LastWeatherData");
                await _cacheManager.SetCacheAsync("LastWeatherData", data, TimeSpan.FromHours(12));
            }
        }
    }
}
