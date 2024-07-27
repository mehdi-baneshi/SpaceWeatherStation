using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWeatherStation.Tests.Integration.WeatherForecastController
{
    public class WeatherForecastControllerTest3 : IClassFixture<WeatherStationApiFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly WeatherStationApiFactory _webAppFactory;

        public WeatherForecastControllerTest3(WeatherStationApiFactory webAppFactory)
        {
            _webAppFactory = webAppFactory;
            _httpClient = webAppFactory.CreateClient();
        }

        [Fact]
        public async Task GetLastForecastData_ReturnsValidWeatherData_WhenApiDownDbUpAndHasRecords()
        {
            //Arange
            var server = _webAppFactory.GetApiServer();
            var container = _webAppFactory.GetDBContainer();
            server.SetupWorkingApi();
            await _httpClient.GetAsync("api/Forecast/GetLastForecastData");
            server.Dispose();
            var newserver = new OpenMeteoFakeAPIServer();
            newserver.Start();
            newserver.SetupDownApi();

            //Act
            var response = await _httpClient.GetAsync("api/Forecast/GetLastForecastData");
            var responseBody = await response.Content.ReadAsStringAsync();

            //Asert
            Assert.True((int)response.StatusCode >= 200 && (int)response.StatusCode <= 204);
            Assert.False(string.IsNullOrEmpty(responseBody));
        }
    }
}
