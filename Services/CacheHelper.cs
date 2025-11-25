namespace CampusEvents.Services;

/// <summary>
/// Utility class for caching operations.
/// Provides simple in-memory caching helpers with expiration support.
/// </summary>
/// <remarks>
/// This class provides a thread-safe, in-memory caching mechanism for storing
/// frequently accessed data with automatic expiration.
/// 
/// Key Features:
/// - Thread-safe operations using locks
/// - Automatic expiration of cached items
/// - Get-or-set pattern for lazy loading
/// - Cache size monitoring
/// - Manual cache invalidation
/// 
/// Important Notes:
/// - This is a simple in-memory cache suitable for development and small deployments
/// - For production, consider using ASP.NET Core's IMemoryCache or IDistributedCache
/// - Cache is stored in a static dictionary, so it persists across requests
/// - Expired items are automatically removed on access or during count operations
/// - All operations are thread-safe using lock synchronization
/// 
/// Limitations:
/// - Memory is not bounded (could grow indefinitely)
/// - No distributed caching support (single server only)
/// - No cache eviction policies (LRU, LFU, etc.)
/// 
/// Example usage:
/// ```csharp
/// // Set a value with 30-minute expiration
/// CacheHelper.Set("key", value, expirationMinutes: 30);
/// 
/// // Get a value
/// var value = CacheHelper.Get<string>("key");
/// 
/// // Get or set (lazy loading)
/// var data = CacheHelper.GetOrSet("key", () => ExpensiveOperation(), 30);
/// 
/// // Remove a value
/// CacheHelper.Remove("key");
/// ```
/// </remarks>
public static class CacheHelper
{
    private static readonly Dictionary<string, (object Value, DateTime Expiry)> _cache = new();
    private static readonly object _lock = new();

    /// <summary>
    /// Gets a value from cache
    /// </summary>
    /// <typeparam name="T">Type of cached value</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached value or default if not found/expired</returns>
    public static T? Get<T>(string key)
    {
        lock (_lock)
        {
            if (!_cache.TryGetValue(key, out var cached))
                return default(T);

            if (cached.Expiry < DateTime.UtcNow)
            {
                _cache.Remove(key);
                return default(T);
            }

            return (T?)cached.Value;
        }
    }

    /// <summary>
    /// Sets a value in cache with expiration
    /// </summary>
    /// <typeparam name="T">Type of value to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expirationMinutes">Expiration time in minutes</param>
    public static void Set<T>(string key, T value, int expirationMinutes = 30)
    {
        lock (_lock)
        {
            var expiry = DateTime.UtcNow.AddMinutes(expirationMinutes);
            _cache[key] = (value!, expiry);
        }
    }

    /// <summary>
    /// Removes a value from cache
    /// </summary>
    /// <param name="key">Cache key to remove</param>
    public static void Remove(string key)
    {
        lock (_lock)
        {
            _cache.Remove(key);
        }
    }

    /// <summary>
    /// Clears all cached values
    /// </summary>
    public static void Clear()
    {
        lock (_lock)
        {
            _cache.Clear();
        }
    }

    /// <summary>
    /// Checks if a key exists in cache and is not expired
    /// </summary>
    /// <param name="key">Cache key to check</param>
    /// <returns>True if key exists and is valid</returns>
    public static bool Exists(string key)
    {
        lock (_lock)
        {
            if (!_cache.TryGetValue(key, out var cached))
                return false;

            if (cached.Expiry < DateTime.UtcNow)
            {
                _cache.Remove(key);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Gets or sets a value in cache (if not exists, executes factory function)
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Factory function to create value if not cached</param>
    /// <param name="expirationMinutes">Expiration time in minutes</param>
    /// <returns>Cached or newly created value</returns>
    public static T GetOrSet<T>(string key, Func<T> factory, int expirationMinutes = 30)
    {
        var cached = Get<T>(key);
        if (cached != null)
            return cached;

        var value = factory();
        Set(key, value, expirationMinutes);
        return value;
    }

    /// <summary>
    /// Gets the number of items currently in cache
    /// </summary>
    /// <returns>Number of cached items</returns>
    public static int Count()
    {
        lock (_lock)
        {
            // Remove expired items first
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.Expiry < DateTime.UtcNow)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.Remove(key);
            }

            return _cache.Count;
        }
    }
}

