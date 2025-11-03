var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<Neo4jService>(
    new Neo4jService("neo4j+s://8bb87af4.databases.neo4j.io", "neo4j", "LP_jKZYCWGDICIaCavzhEOfNlfcr6A1k9-TYO15eHb0"));
builder.Services.AddSingleton<MongoService>(sp =>
        new MongoService("mongodb+srv://anitaal1711_db_user:DZptn5BLaswBcmDk@krvnijevodadb.4kkb5s5.mongodb.net/", "KrvNijeVodaDB"));

new MongoService("mongodb://localhost:27017", "KrvNijeVodaDB");
var redisService = new RedisService(
    "redis-12982.c300.eu-central-1-1.ec2.redns.redis-cloud.com",
    12982,
    "default",
    "9BoltGO34yWtZwsJIBVKOSYCU2D0JdnG"
);

builder.Services.AddSingleton(redisService);



builder.Services.AddScoped<GodinaService>();
builder.Services.AddScoped<ZemljaService>();
builder.Services.AddScoped<RatService>();
builder.Services.AddScoped<DinastijaService>();
//builder.Services.AddScoped<LokacijaService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Your frontend address
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



builder.Services.AddControllers();
// builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await redisService.SetAsync("test-key", "hello world");

    var value = await redisService.GetAsync("test-key");

    Console.WriteLine($"Redis returned: {value}");
}

// class Program
// {
//     static void Main(string[] args)
//     {
// var redisService = new RedisService();
// redisService.run();
//     }
// }
// var redisService = new RedisService(
//     "redis-12982.c300.eu-central-1-1.ec2.redns.redis-cloud.com",
//     12982,
//     "default",
//     "9BoltGO34yWtZwsJIBVKOSYCU2D0JdnG"
// );

// Optional: Run the test once
//redisService.RunTest(); // or await redisService.RunTestAsync() if async

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
