﻿using Microsoft.AspNetCore.Mvc.Testing;
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
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _httpClient.GetAsync("api/Forecast/GetLastForecastData");
            watch.Stop();
            var responseBody = await response.Content.ReadAsStringAsync();
            var elapsedMs = watch.ElapsedMilliseconds;

            //Asert
            Assert.True((int)response.StatusCode == 200);
            Assert.False(string.IsNullOrEmpty(responseBody));
            Assert.True(elapsedMs<1000);
        }
    }
}
