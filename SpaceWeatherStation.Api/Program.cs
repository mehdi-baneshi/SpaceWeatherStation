
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using SpaceWeatherStation.Controllers;
using SpaceWeatherStation.Database;
using SpaceWeatherStation.Factory;
using SpaceWeatherStation.Interfaces;
using SpaceWeatherStation.Services;
using System;
using System.Net;
using Quartz;
using SpaceWeatherStation.BackgroundJobs;
using SpaceWeatherStation.Cache;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace SpaceWeatherStation
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddHttpClient("OpenMeteoAPI", client =>
            {
                client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
                client.Timeout = TimeSpan.FromMilliseconds(1000);
            });

            builder.Services.AddQuartz(q =>
            {
                q.AddJob<CacheWeatherDataJob>(opts => opts.WithIdentity("CacheJob"));
                q.AddTrigger(opts => opts
                    .ForJob("CacheJob")
                    //.StartNow()
                    .StartAt(DateBuilder.FutureDate(1, IntervalUnit.Minute))
                    .WithIdentity("CacheJobTrigger")
                    .WithSimpleSchedule(x => x
                        .WithInterval(TimeSpan.FromMinutes(60))
                        .RepeatForever()));

                q.AddJob<CleanupMainTableJob>(opts => opts.WithIdentity("CleanupDBJob"));
                q.AddTrigger(opts => opts
                    .ForJob("CleanupDBJob")
                    .WithIdentity("CleanupDBJobTrigger")
                    .WithCronSchedule("0 30 * * * ?"));
            });
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            builder.Services.AddRateLimiter(_ =>
            {
                _.AddConcurrencyLimiter("GeneralLimit", options =>
                {
                    options.PermitLimit = 100;
                    options.QueueLimit = 20;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });
                _.RejectionStatusCode = 429;
            });

            builder.Services.AddScoped<DatabaseInitializer>();
            builder.Services.AddScoped<IDbConnectionFactory>(_ =>
                new DbConnectionFactory(builder.Configuration.GetConnectionString("SQLServerConnection"), builder.Configuration.GetConnectionString("SQLServerMasterConnection")));
            builder.Services.AddSingleton<ICircuitBreakerFactory, CircuitBreakerFactory>();
            builder.Services.AddScoped<IExternalDataService, ExternalDataService>();
            builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();
            builder.Services.AddScoped<IApplicationDataService, ApplicationDataService>();
            builder.Services.AddScoped<ICacheManager, CacheManager>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseRateLimiter();
           
            app.MapControllers();

            using var scope = app.Services.CreateScope();
            var databaseInitializer = scope.ServiceProvider.GetService<DatabaseInitializer>();
            await databaseInitializer.InitializeAsync();

            app.Run();
        }
    }
}
