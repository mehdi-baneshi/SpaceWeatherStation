using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWeatherStation.Tests.Integration.WeatherForecastController
{
    public class WeatherForecastControllerTest2 : IClassFixture<WeatherStationApiFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly WeatherStationApiFactory _webAppFactory;

        public WeatherForecastControllerTest2(WeatherStationApiFactory webAppFactory)
        {
            _webAppFactory = webAppFactory;
            _httpClient = webAppFactory.CreateClient();
        }


        [Fact]
        public async Task GetLastForecastData_ReturnsNull_WhenApiDownDbUpAndHasNoRecords()
        {
            //Arange
            var server = _webAppFactory.GetApiServer();
            var container = _webAppFactory.GetDBContainer();
            server.SetupDownApi();

            //Act
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _httpClient.GetAsync("api/Forecast/GetLastForecastData");
            var responseBody = await response.Content.ReadAsStringAsync();

            //Asert
            Assert.True((int)response.StatusCode == 204);
            Assert.True(string.IsNullOrEmpty(responseBody));
        }
    }
}
