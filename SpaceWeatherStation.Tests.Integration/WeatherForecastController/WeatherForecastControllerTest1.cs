using Microsoft.AspNetCore.Mvc.Testing;
using SpaceWeatherStation.Marker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWeatherStation.Tests.Integration.WeatherForecastController
{
    public class WeatherForecastControllerTest1:IClassFixture<WeatherStationApiFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly WeatherStationApiFactory _webAppFactory;

        public WeatherForecastControllerTest1(WeatherStationApiFactory webAppFactory)
        {
            _webAppFactory = webAppFactory;
            _httpClient = webAppFactory.CreateClient();
        }


        [Fact]
        public async Task GetLastForecastData_ReturnsValidWeatherData_WhenApiUp()
        {
            //Arange
            var server = _webAppFactory.GetApiServer();
            var container= _webAppFactory.GetDBContainer();
            server.SetupWorkingApi();

            //Act
            var response = await _httpClient.GetAsync("api/Forecast/GetLastForecastData");
            var responseBody = await response.Content.ReadAsStringAsync();

            await Task.Delay(30000);

            //Asert
            Assert.True((int)response.StatusCode >= 200 && (int)response.StatusCode <= 204);
            Assert.False(string.IsNullOrEmpty(responseBody));
        }
    }
}
