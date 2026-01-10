using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Neo4jClient;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;



// MongoDB client
var mongoClient = new MongoClient("mongodb+srv://anitaal1711_db_user:DZptn5BLaswBcmDk@krvnijevodadb.4kkb5s5.mongodb.net/");
var mongoDatabase = mongoClient.GetDatabase("KrvNijeVodaDB");

// Register the collections for DI
builder.Services.AddSingleton<IMongoCollection<VladarMongo>>(sp =>
    mongoDatabase.GetCollection<VladarMongo>("Vladari"));

builder.Services.AddSingleton<IMongoCollection<LicnostMongo>>(sp =>
    mongoDatabase.GetCollection<LicnostMongo>("Licnosti"));


// Register Neo4jClient IGraphClient
builder.Services.AddSingleton<IGraphClient>(sp =>
{
    var client = new BoltGraphClient(
        "neo4j+s://8bb87af4.databases.neo4j.io", // same as your Neo4jService URL
        "neo4j",
        "LP_jKZYCWGDICIaCavzhEOfNlfcr6A1k9-TYO15eHb0"
    );

    // Connect on startup (synchronously)
    client.ConnectAsync().Wait();

    return client;
});

builder.Services.AddSingleton<Neo4jService>(
    new Neo4jService("neo4j+s://8bb87af4.databases.neo4j.io", "neo4j", "LP_jKZYCWGDICIaCavzhEOfNlfcr6A1k9-TYO15eHb0"));
builder.Services.AddSingleton<MongoService>(sp =>
        new MongoService("mongodb+srv://anitaal1711_db_user:DZptn5BLaswBcmDk@krvnijevodadb.4kkb5s5.mongodb.net/", "KrvNijeVodaDB"));

new MongoService("mongodb://localhost:27017", "KrvNijeVodaDB");
builder.Services.AddSingleton<RedisService>(sp =>
    new RedisService(
        "redis-10165.c300.eu-central-1-1.ec2.cloud.redislabs.com",
        10165,
        "default",
        "u2CMbepHd3ojAmph1vYgQNq0SRbqzCHB"
    )
);

builder.Services.AddScoped<GodinaService>();
builder.Services.AddScoped<ZemljaService>();
builder.Services.AddScoped<RatService>();
builder.Services.AddScoped<DinastijaService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IVladarService, VladarService>();
builder.Services.AddScoped<ILicnostService, LicnostService>();
builder.Services.AddScoped<ITreeBuilder, TreeBuilder>();
//builder.Services.AddScoped<LokacijaService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") //frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.IncludeErrorDetails = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,

        RoleClaimType = ClaimTypes.Role,
        NameClaimType = JwtRegisteredClaimNames.Sub
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("JWT failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("JWT validated for: " + context.Principal.Identity.Name);
            foreach(var c in context.Principal.Claims)
                Console.WriteLine($"{c.Type}: {c.Value}");
            return Task.CompletedTask;
        }
    };

});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(IdentityData.AdminPolicyName, p =>
    p.RequireRole(IdentityData.AdminRoleName));
});



builder.Services.AddControllers();
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseCors();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
//app.UseMiddleware<RequestTrackingMiddleware>();



app.MapControllers();

app.Run();
