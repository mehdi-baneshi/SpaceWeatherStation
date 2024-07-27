using Polly.CircuitBreaker;

namespace SpaceWeatherStation.Interfaces
{
    public interface ICircuitBreakerFactory
    {
        public AsyncCircuitBreakerPolicy<HttpResponseMessage> GetAPICircuitBreakerPolicy();
        public AsyncCircuitBreakerPolicy GetDBCircuitBreakerPolicy();
    }
}
