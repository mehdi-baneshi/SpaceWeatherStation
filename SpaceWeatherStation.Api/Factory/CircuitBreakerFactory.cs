using Microsoft.Data.SqlClient;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using SpaceWeatherStation.Interfaces;
using System.ComponentModel;
using System.Net;

namespace SpaceWeatherStation.Factory
{
    public class CircuitBreakerFactory: ICircuitBreakerFactory
    {
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _APICircuitBreakerPolicy;
        private readonly AsyncCircuitBreakerPolicy _DBCircuitBreakerPolicy;
        public CircuitBreakerFactory()
        {
            _APICircuitBreakerPolicy = Policy<HttpResponseMessage>.Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult(x => x.StatusCode >= HttpStatusCode.InternalServerError || x.StatusCode == HttpStatusCode.RequestTimeout)
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(60));

            _DBCircuitBreakerPolicy = Policy.Handle<SqlException>()
                                .Or<TimeoutRejectedException>()
                                .Or<Win32Exception>()
                                .Or<TaskCanceledException>()
                                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(2));
        }

        public AsyncCircuitBreakerPolicy<HttpResponseMessage> GetAPICircuitBreakerPolicy()
        {
            return _APICircuitBreakerPolicy;
        }

        public AsyncCircuitBreakerPolicy GetDBCircuitBreakerPolicy()
        {
            return _DBCircuitBreakerPolicy;
        }
    }
}
