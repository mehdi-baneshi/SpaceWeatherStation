using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SpaceWeatherStation.Entities;
using SpaceWeatherStation.Interfaces;
using System.Runtime.InteropServices;

namespace SpaceWeatherStation.Controllers
{
    [EnableRateLimiting("GeneralLimit")]
    [ApiController]
    [Route("api/[controller]")]
    public class ForecastController : ControllerBase
    {
        private readonly IExternalDataService _externalDataService;
        private readonly IApplicationDataService _applicationDataService;

        public ForecastController(IExternalDataService externalDataService, IApplicationDataService applicationDataService)
        {
            _externalDataService = externalDataService;
            _applicationDataService = applicationDataService;
        }

        [HttpGet("GetLastForecastData")]
        public async Task<IActionResult> GetLastForecastData()
        {
            string validData;

            var apiData=await _externalDataService.GetForecastDataFromWebService();
            if (apiData is not null)
            {
                validData = apiData;
                var insertTask = _applicationDataService.InsertWeatherData(new APIWeatherData
                {
                    JsonValue = validData,
                    APIResponseDate = DateTime.Now,
                });
            }
            else
            {
                var dbdata = await _applicationDataService.GetLastWeatherDataFromDatabase();
                if (dbdata is not null)
                {
                    validData = dbdata;
                }
                else
                {
                    var cachedData = await _applicationDataService.GetLastWeatherDataFromCache();
                    validData = cachedData;
                }
            }

            return Ok(validData);
        }


        //[HttpGet("GetLastForecastDataAlternativeStrategy")]
        //public async Task<IActionResult> GetLastForecastDataAlternativeStrategy()
        //{
        //    string validData = "";

        //    var apiDataTask = _externalDataService.GetForecastDataFromWebService();
        //    var delayTask = Task.Delay(2000);

        //    var winnerTask = await Task.WhenAny(apiDataTask, delayTask);

        //    if (winnerTask== apiDataTask)
        //    {
        //        string data = await apiDataTask;
        //        if (data == null)
        //        {
        //            var dbdata = await _applicationDataService.GetLastWeatherDataFromDatabase();
        //            if (dbdata is null)
        //            {
        //                var cachedData=await _applicationDataService.GetLastWeatherDataFromCache();
        //                validData = cachedData;
        //            }
        //            else
        //            {
        //                validData = dbdata;
        //            }
        //        }
        //        else
        //        {
        //            validData = data;

        //            var insertTask = _applicationDataService.InsertWeatherData(new APIWeatherData
        //            {
        //                JsonValue = validData,
        //                APIResponseDate = DateTime.Now,
        //            });
        //        }
        //    }
        //    else
        //    {
        //        var dbDataTask = _applicationDataService.GetLastWeatherDataFromDatabase();
        //        string data = await dbDataTask;
        //        if (data==null)
        //        {
        //            var dbdata=await dbDataTask;
        //            if (dbdata is null)
        //            {
        //                var cachedData = await _applicationDataService.GetLastWeatherDataFromCache();
        //                validData = cachedData;
        //            }
        //            else
        //            {
        //                validData = dbdata;
        //            }
        //        }
        //        else
        //        {
        //            validData = data;

        //            var insertTask = _applicationDataService.InsertWeatherData(new APIWeatherData
        //            {
        //                JsonValue = validData,
        //                APIResponseDate = DateTime.Now,
        //            });
        //        }
        //    }

        //    return Ok(validData);
        //}
    }
}
