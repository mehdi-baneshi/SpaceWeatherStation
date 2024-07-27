using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using SpaceWeatherStation.Interfaces;
using System.Net;
using System.Net.Http;

namespace SpaceWeatherStation.Services
{
    public class ExternalDataService: IExternalDataService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICircuitBreakerFactory _circuitBreakerFactory;

        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy= Policy<HttpResponseMessage>.Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult(x => x.StatusCode >= HttpStatusCode.InternalServerError || x.StatusCode == HttpStatusCode.RequestTimeout)
            .WaitAndRetryAsync(10, times => TimeSpan.FromMilliseconds(300));

        private readonly IAsyncPolicy<HttpResponseMessage> _timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(3);


        public ExternalDataService(IHttpClientFactory httpClientFactory, ICircuitBreakerFactory circuitBreakerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _circuitBreakerFactory = circuitBreakerFactory;
        }

        public async Task<string> GetForecastDataFromWebService()
        {
            var circuitBreakerPolicy = _circuitBreakerFactory.GetAPICircuitBreakerPolicy();
            var fullPolicy = Policy.WrapAsync(_timeoutPolicy, circuitBreakerPolicy, _retryPolicy);

            if (circuitBreakerPolicy.CircuitState==CircuitState.Open)
            {
                return null;
            }

            var client = _httpClientFactory.CreateClient("OpenMeteoAPI");

            try
            {
                var res = await fullPolicy.ExecuteAsync(async () =>
                    await client.GetAsync("forecast?latitude=52.52&longitude=13.41&hourly=temperature_2m,relativehumidity_2m,windspeed_10m"));

                res.EnsureSuccessStatusCode();

                var responseBody = await res.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (Exception)
            {
                return null;
            }
            
        }
    }
}
