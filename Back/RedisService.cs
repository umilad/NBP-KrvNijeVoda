using StackExchange.Redis;
using System;
using System.Threading.Tasks;
public class RedisService
{
    private readonly IDatabase _db;

    public RedisService(string host, int port, string user, string password)
    {
        try
        {
            var connectionString = $"rediss://{user}:{password}@{host}:{port},abortConnect=false";

            var muxer = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { { "redis-10165.c300.eu-central-1-1.ec2.cloud.redislabs.com", 10165 } },
                User = "default",
                Password = "u2CMbepHd3ojAmph1vYgQNq0SRbqzCHB"
            }
        );

            
            _db = muxer.GetDatabase();

            Console.WriteLine("Connected to Redis successfully!");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis connection failed: {ex.Message}");
            _db = null;
        }
    }


    private string GetCategoryFromPath(string path)
{
    if (string.IsNullOrWhiteSpace(path))
        return "ostalo";

    if (path.StartsWith("/dogadjaj", StringComparison.OrdinalIgnoreCase))
        return "dogadjaj";

    if (path.StartsWith("/licnost", StringComparison.OrdinalIgnoreCase))
        return "licnost";

    if (path.StartsWith("/dinastija", StringComparison.OrdinalIgnoreCase))
        return "dinastija";

    return "ostalo";
}


    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (_db == null) return;
        await _db.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetAsync(string key)
    {
        if (_db == null) return null;
        return await _db.StringGetAsync(key);
    }

    public async Task RunTestAsync()
    {
        await _db.StringSetAsync("foo", "bar");
        await _db.StringSetAsync("foo2", "bar2");
        var result = await _db.StringGetAsync("foo");
        Console.WriteLine($"Redis test value: {result}");
    }

    public async Task<bool> DeleteAsync(string key)
    {
        if (_db == null) return false;
        return await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (_db == null) return false;
        return await _db.KeyExistsAsync(key);
    }

    public async Task AddVisitedPageAsync(string token, string page)
    {
        if (_db == null) return;
        await _db.ListRightPushAsync($"{token}:history", page);
        await _db.KeyExpireAsync($"{token}:history", TimeSpan.FromHours(2));
    }

    public async Task<List<string>> GetVisitedPagesAsync(string token)
    {
        if (_db == null) return new List<string>();
        var values = await _db.ListRangeAsync($"{token}:history");
        return values.Select(v => v.ToString()).ToList();
    }

    public async Task DeleteHistoryAsync(string token)
    {
        if (_db == null) return;
        await _db.KeyDeleteAsync($"{token}:history");
    }

public async Task SetHashAsync(string key, Dictionary<string, int> values, TimeSpan? expiry = null)
{
    if (_db == null) return;
    
    if (values != null && values.Count > 0)
    {
        var entries = values.Select(v => new HashEntry(v.Key, v.Value)).ToArray();
        await _db.HashSetAsync(key, entries);
    }

    if (expiry.HasValue)
        await _db.KeyExpireAsync(key, expiry);
}

 public async Task IncrementPageVisitAsync(string username, string path, string label)
{
    if (_db == null) return;

    string key = username;

    var keyType = await _db.KeyTypeAsync(key);
    if (keyType != RedisType.Hash)
    {
        await _db.KeyDeleteAsync(key);
        await _db.HashSetAsync(key, Array.Empty<HashEntry>());
    }

    var existing = await _db.HashGetAsync(key, path);
    int count = 1;

    if (!existing.IsNullOrEmpty)
    {
        var parts = existing.ToString().Split('|');
        if (parts.Length > 0 && int.TryParse(parts[0], out var c))
            count = c + 1;
    }

    string finalLabel = string.IsNullOrWhiteSpace(label) ? path : label;

    await _db.HashSetAsync(key, path, $"{count}|{finalLabel}");
    await _db.KeyExpireAsync(key, TimeSpan.FromHours(12));

    await _db.HashSetAsync("stats:pages:labels", path, finalLabel);

    await _db.SortedSetIncrementAsync(
        "stats:pages:global",
        path,
        1
    );
}
public class PageStat
{
    public string Path { get; set; }
    public int Count { get; set; }
    public string Label { get; set; }
}

public async Task<List<PageStat>> GetGlobalTopByCategoryAsync(string category, int top = 10)
{
    if (_db == null) return new List<PageStat>();

    var entries = await _db.SortedSetRangeByRankWithScoresAsync(
        "stats:pages:global",
        0,
        100, // buffer
        Order.Descending
    );

    var result = new List<PageStat>();

    foreach (var e in entries)
    {
        string path = e.Element.ToString();

        if (!path.StartsWith($"/{category}", StringComparison.OrdinalIgnoreCase))
            continue;

        var label = await _db.HashGetAsync("stats:pages:labels", path);

        result.Add(new PageStat
        {
            Path = path,
            Count = (int)e.Score,
            Label = label.HasValue ? label.ToString() : path
        });

        if (result.Count >= top)
            break;
    }

    return result;
}

    public async Task<List<object>> GetTopVisitedPagesAsync(string username)
    {
        if (_db == null) return new List<object>();

        string key = username;
        var all = await _db.HashGetAllAsync(key);

        return all
            .Select(x =>
            {
                var parts = x.Value.ToString().Split('|');
                int count = parts.Length > 0 && int.TryParse(parts[0], out var c) ? c : 0;
                string label = parts.Length > 1 ? parts[1] : x.Name.ToString();
                return new { Path = x.Name.ToString(), Count = count, Label = label };
            })
            .OrderByDescending(x => x.Count)
            .Cast<object>()
            .ToList();
    }

public async Task<List<PageStat>> GetGlobalTopPagesAsync(int top = 10)
{
    if (_db == null) return new List<PageStat>();

    var entries = await _db.SortedSetRangeByRankWithScoresAsync(
        "stats:pages:global",
        0,
        100,
        Order.Descending
    );

    var result = new List<PageStat>();

    foreach (var e in entries)
    {
        string path = e.Element.ToString();
        int count = (int)e.Score;

        var label = await _db.HashGetAsync("stats:pages:labels", path);
        string finalLabel = label.HasValue ? label.ToString() : path;

        result.Add(new PageStat
        {
            Path = path,
            Count = count,
            Label = finalLabel
        });

        if (result.Count >= top)
            break;
    }

    return result;
}


}
