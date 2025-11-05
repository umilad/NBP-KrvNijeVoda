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
            var muxer = ConnectionMultiplexer.Connect( new ConfigurationOptions{
                EndPoints= { {"redis-13125.c311.eu-central-1-1.ec2.redns.redis-cloud.com", 13125} },
                User="default",
                Password="olHTtzdeV5iMAuV081w4jWAwRZIRiLkR"
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
