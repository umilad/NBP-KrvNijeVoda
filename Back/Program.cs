using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<Neo4jService>(
    new Neo4jService("neo4j+s://8bb87af4.databases.neo4j.io", "neo4j", "LP_jKZYCWGDICIaCavzhEOfNlfcr6A1k9-TYO15eHb0"));
builder.Services.AddSingleton<MongoService>(sp =>
        new MongoService("mongodb+srv://anitaal1711_db_user:DZptn5BLaswBcmDk@krvnijevodadb.4kkb5s5.mongodb.net/", "KrvNijeVodaDB"));

new MongoService("mongodb://localhost:27017", "KrvNijeVodaDB");
var redisService = new RedisService(
    "redis-13125.c311.eu-central-1-1.ec2.redns.redis-cloud.com",
    13125,
    "default",
    "olHTtzdeV5iMAuV081w4jWAwRZIRiLkR"
);

builder.Services.AddSingleton(redisService);



builder.Services.AddScoped<GodinaService>();
builder.Services.AddScoped<ZemljaService>();
builder.Services.AddScoped<RatService>();
builder.Services.AddScoped<DinastijaService>();
builder.Services.AddSingleton<TokenService>();
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

var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var redis = context.HttpContext.RequestServices.GetRequiredService<RedisService>();
            var token = context.SecurityToken as JwtSecurityToken;
            if (token == null)
            {
                context.Fail("Invalid token");
                return;
            }

            // Check if token exists in Redis
            var exists = await redis.ExistsAsync(token.RawData); // only token string
            if (!exists)
            {
                context.Fail("Token not found in Redis (logged out?)");
            }
        }
    };
});

builder.Services.AddAuthorization();



builder.Services.AddControllers();
// builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token.\r\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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

app.UseAuthentication();
app.UseAuthorization();



app.MapControllers();

app.Run();
