using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWeatherStation.Tests.Integration.WeatherForecastController
{
    public class WeatherForecastControllerTest6 : IClassFixture<WeatherStationApiFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly WeatherStationApiFactory _webAppFactory;

        public WeatherForecastControllerTest6(WeatherStationApiFactory webAppFactory)
        {
            _webAppFactory = webAppFactory;
            _httpClient = webAppFactory.CreateClient();
        }

        [Fact]
        public async Task GetLastForecastData_ReturnsImmediateValidWeatherData_WhenApiDownDbUPAndCircuitBreakerOpen()
        {
            //Arange
            var server = _webAppFactory.GetApiServer();
            server.SetUpWorkingThenDownApi();
            await _httpClient.GetAsync("api/Forecast/GetLastForecastData"); //api runs and record is saved in db
            await _httpClient.GetAsync("api/Forecast/GetLastForecastData"); //api is down
            await _httpClient.GetAsync("api/Forecast/GetLastForecastData"); //api is down and circuit breaker opens

            //Act
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _httpClient.GetAsync("api/Forecast/GetLastForecastData");
            watch.Stop();
            var responseBody = await response.Content.ReadAsStringAsync();
            var elapsedMs = watch.ElapsedMilliseconds;

            //Asert
            Assert.True((int)response.StatusCode == 200);
            Assert.False(string.IsNullOrEmpty(responseBody));
            Assert.True(elapsedMs < 500);
        }
    }
}
