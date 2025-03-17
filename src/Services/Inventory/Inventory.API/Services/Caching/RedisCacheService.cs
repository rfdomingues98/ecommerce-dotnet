using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;


namespace Inventory.API.Services.Caching;

public interface ICacheService
{
    Task<T> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
    Task RemoveAsync(string key);
}

public class RedisCacheService : ICacheService
{

    private readonly IDatabase _database;

    public RedisCacheService(IConfiguration configuration)
    {
        var redis = ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]);
        _database = redis.GetDatabase();
    }

    public async Task<T> GetAsync<T>(string key) where T : class
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, serializedValue, expiry);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

}