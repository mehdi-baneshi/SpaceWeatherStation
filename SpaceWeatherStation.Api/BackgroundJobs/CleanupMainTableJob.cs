using Quartz;
using SpaceWeatherStation.Interfaces;

namespace SpaceWeatherStation.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class CleanupMainTableJob : IJob
    {
        private readonly IApplicationDataService _applicationDataService;

        public CleanupMainTableJob(IApplicationDataService applicationDataService)
        {
            _applicationDataService = applicationDataService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _applicationDataService.ArchiveOldData();
        }
    }
}
