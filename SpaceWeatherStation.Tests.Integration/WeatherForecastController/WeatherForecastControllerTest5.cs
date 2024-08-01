using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWeatherStation.Tests.Integration.WeatherForecastController
{
    public class WeatherForecastControllerTest5 : IClassFixture<WeatherStationApiFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly WeatherStationApiFactory _webAppFactory;

        public WeatherForecastControllerTest5(WeatherStationApiFactory webAppFactory)
        {
            _webAppFactory = webAppFactory;
            _httpClient = webAppFactory.CreateClient();
        }

        [Fact]
        public async Task GetLastForecastData_ReturnsNull_WhenApiDownDBDownAndCacheEmpty()
        {
            //Arange
            var server = _webAppFactory.GetApiServer();
            var dbcontainer = _webAppFactory.GetDBContainer();
            server.SetupDownApi();
            await dbcontainer.StopAsync();

            //Act
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _httpClient.GetAsync("api/Forecast/GetLastForecastData");
            watch.Stop();
            var responseBody = await response.Content.ReadAsStringAsync();
            var elapsedMs = watch.ElapsedMilliseconds;

            //Asert
            Assert.True((int)response.StatusCode == 204);
            Assert.True(string.IsNullOrEmpty(responseBody));
            Assert.True(elapsedMs > 4500 && elapsedMs < 5000);
        }
    }
}
