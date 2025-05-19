var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<Neo4jService>(new Neo4jService("neo4j+s://90ad2e70.databases.neo4j.io", "neo4j", "jGYYLQH3Tdc33sNGdSmpAomQChOhAolLE3mwG-IR4M4"));
//builder.Services.AddScoped<GodinaService>();
//builder.Services.AddScoped<LokacijaService>();
//builder.Services.AddScoped<ZemljaService>();
//builder.Services.AddScoped<RatService>();
//builder.Services.AddScoped<DinastijaService>();
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
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
