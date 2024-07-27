using Quartz;
using SpaceWeatherStation.Cache;
using SpaceWeatherStation.Interfaces;
using System.Runtime.InteropServices;

namespace SpaceWeatherStation.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class CacheWeatherDataJob : IJob
    {
        private readonly IApplicationDataService _applicationDataService;
        private readonly ICacheManager _cacheManager;

        public CacheWeatherDataJob(IApplicationDataService applicationDataService, ICacheManager cacheManager)
        {
            _applicationDataService = applicationDataService;
            _cacheManager = cacheManager;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var data = await _applicationDataService.GetLastWeatherDataFromDatabase();
            if (data is not null)
            {
                await _cacheManager.RemoveCacheAsync("LastWeatherData");
                await _cacheManager.SetCacheAsync("LastWeatherData", data, TimeSpan.FromHours(12));
            }
        }
    }
}
