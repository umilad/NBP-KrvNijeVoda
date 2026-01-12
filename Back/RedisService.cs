using StackExchange.Redis;
using System;
using System.Threading.Tasks;

public class RedisService
{
    private readonly IDatabase _db;

    // Constructor takes host, port, password
    public RedisService(string host, int port, string user, string password)
    {
        try
        {
            // Connection string with abortConnect=false
            var connectionString = $"rediss://{user}:{password}@{host}:{port},abortConnect=false";

            // Connect to Redis
            var muxer = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { { "redis-10165.c300.eu-central-1-1.ec2.cloud.redislabs.com", 10165 } },
                User = "default",
                Password = "u2CMbepHd3ojAmph1vYgQNq0SRbqzCHB"
            }
        );

            // Get the database
            _db = muxer.GetDatabase();

            Console.WriteLine("Connected to Redis successfully!");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis connection failed: {ex.Message}");
            _db = null; // optional: mark as unavailable
        }
    }




    // Set a value in Redis
    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (_db == null) return; // skip if Redis unavailable
        await _db.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetAsync(string key)
    {
        if (_db == null) return null;
        return await _db.StringGetAsync(key);
    }

    // Async test method
    public async Task RunTestAsync()
    {
        await _db.StringSetAsync("foo", "bar");
        await _db.StringSetAsync("foo2", "bar2");
        var result = await _db.StringGetAsync("foo");
        Console.WriteLine($"Redis test value: {result}"); // shows bar in console
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
        await _db.KeyExpireAsync($"{token}:history", TimeSpan.FromHours(2)); // koliko traje token
    }

    // Vrati listu stranica za dati token
    public async Task<List<string>> GetVisitedPagesAsync(string token)
    {
        if (_db == null) return new List<string>();
        var values = await _db.ListRangeAsync($"{token}:history");
        return values.Select(v => v.ToString()).ToList();
    }

    // Obriši istoriju kad se token obriše
    public async Task DeleteHistoryAsync(string token)
    {
        if (_db == null) return;
        await _db.KeyDeleteAsync($"{token}:history");
    }

    // 🟩 Inicijalizuj prazan hash
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




// 🟩 Povećaj brojač poseta, sada sa label
  // Povećaj brojač poseta i sačuvaj label
 public async Task IncrementPageVisitAsync(string username, string path, string label)
{
    if (_db == null) return;

    string key = username;

    var keyType = await _db.KeyTypeAsync(key);
    if (keyType != RedisType.Hash)
    {
        await _db.KeyDeleteAsync(key);
        await _db.HashSetAsync(key, new HashEntry[] { });
    }

    var existing = await _db.HashGetAsync(key, path);
    int count = 1;
    if (!existing.IsNullOrEmpty)
    {
        var parts = existing.ToString().Split('|');
        if (parts.Length > 0 && int.TryParse(parts[0], out var existingCount))
            count = existingCount + 1;
    }

    string newValue = $"{count}|{label}";
    await _db.HashSetAsync(key, path, newValue);
    await _db.KeyExpireAsync(key, TimeSpan.FromHours(12));

    // ⚡️ GLOBAL TRACKING
    await _db.HashSetAsync("stats:pages:labels", path, label ?? path);

    await _db.SortedSetIncrementAsync(
        "stats:pages:global",
        path,
        1
    );
}



    // Vrati top posete sa label
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

public async Task<List<object>> GetGlobalTopPagesAsync(int top = 10)
{
    if (_db == null) return new List<object>();

    var entries = await _db.SortedSetRangeByRankWithScoresAsync(
        "stats:pages:global",
        0,
        top - 1,
        Order.Descending
    );

    var result = new List<object>();

    foreach (var e in entries)
    {
        string path = e.Element.ToString();
        int count = (int)e.Score;

        // Traži label u nekom "global labels" hash-u ili pokuša iz korisnika
        var globalLabel = await _db.HashGetAsync("stats:pages:labels", path);
        string label = globalLabel.HasValue ? globalLabel.ToString() : path;

        result.Add(new { Path = path, Count = count, Label = label });
    }

    return result;
}


}







// using StackExchange.Redis;
// using System;
// using System.Threading.Tasks;

// public class RedisService
// {
//     private readonly IDatabase _db;

//     // Constructor that takes 4 arguments
//     public RedisService(string host, int port, string user, string password)
//     {
//         var config = new ConfigurationOptions
//         {
//             EndPoints = { { host, port } },
//             User = user,
//             Password = password,
//             Ssl = true,        // Redis Cloud usually requires SSL
//             AbortOnConnectFail = false
//         };

//         var muxer = ConnectionMultiplexer.Connect(config);
//         _db = muxer.GetDatabase();
//     }

//     // Async method to set a value
//     public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
//     {
//         await _db.StringSetAsync(key, value, expiry);
//     }

//     // Async method to get a value
//     public async Task<string?> GetAsync(string key)
//     {
//         return await _db.StringGetAsync(key);
//     }

//     // Optional: simple test method like your run()
//     public void RunTest()
//     {
//         _db.StringSet("foo", "bar");
//         var result = _db.StringGet("foo");
//         Console.WriteLine(result); // should print "bar"
//     }
// }


// using NRedisStack;
// using NRedisStack.RedisStackCommands;
// using StackExchange.Redis;

// public class RedisService
// {
//     public void run()
//     {
//         var muxer = ConnectionMultiplexer.Connect(
//             new ConfigurationOptions{
//                 EndPoints= { {"redis-12982.c300.eu-central-1-1.ec2.redns.redis-cloud.com", 12982} },
//                 User="default",
//                 Password="9BoltGO34yWtZwsJIBVKOSYCU2D0JdnG"
//             }
//         );
//         var db = muxer.GetDatabase();
        
//         db.StringSet("foo", "bar");
//         RedisValue result = db.StringGet("foo");
//         Console.WriteLine(result); // >>> bar
        
//     }
// }




// using StackExchange.Redis;

// public class RedisService
// {
//     private readonly IDatabase _db;

//     public RedisService(string connectionString)
//     {
//         var redis = ConnectionMultiplexer.Connect(connectionString);
//         _db = redis.GetDatabase();
//     }

//     public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
//     {
//         await _db.StringSetAsync(key, value, expiry);
//     }

//     public async Task<string?> GetAsync(string key)
//     {
//         return await _db.StringGetAsync(key);
//     }
// }
 