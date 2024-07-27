namespace SpaceWeatherStation.Interfaces
{
    public interface ICacheManager
    {
        public Task SetCacheAsync<T>(string key, T value, TimeSpan expirationTime);
        public Task<T> GetCacheAsync<T>(string key);
        public Task RemoveCacheAsync(string key);
    }
}
