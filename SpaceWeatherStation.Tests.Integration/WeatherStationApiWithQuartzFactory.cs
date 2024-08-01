using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SpaceWeatherStation.Marker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Xunit;
using SpaceWeatherStation.Interfaces;
using SpaceWeatherStation.Database;
using Testcontainers.MsSql;
using Microsoft.Data.SqlClient;
using Quartz;
using SpaceWeatherStation.BackgroundJobs;
using Quartz.Spi;
using Microsoft.Extensions.Hosting;

namespace SpaceWeatherStation.Tests.Integration
{
    public class WeatherStationApiWithQuartzFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
    {

        private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder().Build();
        private readonly OpenMeteoFakeAPIServer _openMeteoFakeAPIServer = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IScheduler));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                var descriptorr = services.SingleOrDefault(d => d.ServiceType == typeof(IJobFactory));
                if (descriptorr != null)
                {
                    services.Remove(descriptorr);
                }
                var quartzServices = services.Where(d => d.ServiceType.Namespace?.StartsWith("Quartz") ?? false).ToList();
                foreach (var service in quartzServices)
                {
                    services.Remove(service);
                }
                services.RemoveAll(typeof(IHostedService));


                services.AddQuartz(q =>
                {
                    string key = Guid.NewGuid().ToString();
                    q.AddJob<CacheWeatherDataJob>(opts => opts.WithIdentity(key));
                    q.AddTrigger(opts => opts
                        .ForJob(key)
                        .StartAt(DateBuilder.FutureDate(10, IntervalUnit.Second))
                        .WithIdentity($"{key}Trigger")
                        .WithSimpleSchedule(x => x
                            .WithInterval(TimeSpan.FromSeconds(10))
                            .RepeatForever()));
                });
                services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


                services.RemoveAll(typeof(IDbConnectionFactory));
                services.AddSingleton<IDbConnectionFactory>(_ =>
                    new DbConnectionFactory(ApplyConnectionTimeout(_msSqlContainer.GetConnectionString()), _msSqlContainer.GetConnectionString()));

                services.AddHttpClient("OpenMeteoAPI", client =>
                {
                    client.BaseAddress = new Uri(_openMeteoFakeAPIServer.Url);
                    client.Timeout = TimeSpan.FromSeconds(1);
                });
            });
        }

        public OpenMeteoFakeAPIServer GetApiServer()
        {
            return _openMeteoFakeAPIServer;
        }

        public MsSqlContainer GetDBContainer()
        {
            return _msSqlContainer;
        }

        public async Task InitializeAsync()
        {
            _openMeteoFakeAPIServer.Start();
            //_openMeteoFakeAPIServer.SetupWorkingApi();
            await _msSqlContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _msSqlContainer.DisposeAsync();
            _openMeteoFakeAPIServer.Dispose();
        }

        private string ApplyConnectionTimeout(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            builder.ConnectTimeout = 1;

            return builder.ConnectionString;
        }
    }
}
