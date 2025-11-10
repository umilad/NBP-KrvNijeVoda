using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MongoService _mongoService;
    private readonly RedisService _redisService;
    private readonly TokenService _tokenService;

    public AuthController(MongoService mongoService, RedisService redisService, TokenService tokenService)
    {
        _mongoService = mongoService;
        _redisService = redisService;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var existing = await _mongoService.Users.Find(u => u.Username == dto.Username).FirstOrDefaultAsync();
        if (existing != null) return BadRequest("Username already exists");

        var user = new UserMongo
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.CustomClaims.Role,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        await _mongoService.Users.InsertOneAsync(user);
        return Ok("User registered");
    }

   [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto loginRequest)
{
    Console.WriteLine("Login request received for: " + loginRequest.Username);

    var user = await _mongoService.Users
        .Find(u => u.Username == loginRequest.Username)
        .FirstOrDefaultAsync();

    Console.WriteLine(user != null ? "User found" : "User not found");

    if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
    {
        Console.WriteLine("Invalid credentials");
        return Unauthorized("Invalid username or password");
    }

    var jwt = _tokenService.GenerateToken(loginRequest);
    Console.WriteLine("JWT token generated: " + jwt);

    var emptyHistory = JsonSerializer.Serialize(new List<string>());
    await _redisService.SetAsync(jwt, emptyHistory, TimeSpan.FromHours(2));
    Console.WriteLine("Empty history stored in Redis for token");

    string userKey = loginRequest.Username;

    bool exists = await _redisService.ExistsAsync(userKey);
    Console.WriteLine($"Redis key exists for user: {exists}");

    if (!exists)
    {
        await _redisService.SetHashAsync(userKey, new Dictionary<string, int>(), TimeSpan.FromHours(12));
        Console.WriteLine("Hash created in Redis for user");
    }

    return Ok(new
    {
        token = jwt,
        expiresIn = _tokenService.TokenLifetime.TotalSeconds,
        role = user.Role
    });
}


    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader)) return BadRequest("Missing token");

        var token = authHeader.Replace("Bearer ", "").Trim();
        if (string.IsNullOrEmpty(token)) return BadRequest("Invalid token");

        await _redisService.DeleteAsync(token);
        return Ok("Logged out successfully, history cleared.");
    }


    // ✅ Novi endpoint za trackovanje frontend ruta
    public class PageDto
    {
        public string Path { get; set; } = "";
        public string Label { get; set; } = "";
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
        if (string.IsNullOrEmpty(token)) return BadRequest("Missing token");

        var value = await _redisService.GetAsync(token);
        if (string.IsNullOrEmpty(value)) return Ok(new List<PageDto>()); // prazna lista

        var pages = JsonSerializer.Deserialize<List<PageDto>>(value, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<PageDto>();

        return Ok(pages);
    }

    [HttpPost("track")]
    public async Task<IActionResult> Track([FromBody] PageDto dto)
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader)) return BadRequest("Missing token");

        var token = authHeader.Replace("Bearer ", "").Trim();
        if (string.IsNullOrEmpty(token)) return BadRequest("Invalid token");

        var value = await _redisService.GetAsync(token);

        var pages = string.IsNullOrEmpty(value)
            ? new List<PageDto>()
            : JsonSerializer.Deserialize<List<PageDto>>(value, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<PageDto>();

        var existing = pages.FirstOrDefault(p => p.Path == dto.Path);
        if (existing != null)
        {
            existing.Label = dto.Label; // update label
        }
        else
        {
            pages.Add(new PageDto { Path = dto.Path, Label = dto.Label });
        }

        await _redisService.SetAsync(token, JsonSerializer.Serialize(pages), TimeSpan.FromHours(2));

        return Ok();
    }

[HttpPost("track-visit")]
public async Task<IActionResult> TrackVisit([FromBody] PageDto dto)
{
    var authHeader = Request.Headers["Authorization"].ToString();
    if (string.IsNullOrEmpty(authHeader))
        return BadRequest("Missing token");

    var token = authHeader.Replace("Bearer ", "").Trim();
    if (string.IsNullOrEmpty(token))
        return BadRequest("Invalid token");

    var username = _tokenService.GetUsernameFromToken(token);
    if (string.IsNullOrEmpty(username))
        return Unauthorized("Invalid or expired token");

    // ⚡️ Prosledi i label
    await _redisService.IncrementPageVisitAsync(username, dto.Path, dto.Label ?? dto.Path);

    return Ok("Visit tracked");
}

// 📊 Dohvatanje najposećenijih stranica korisnika
[HttpGet("top-visits")]
public async Task<IActionResult> GetTopVisits()
{
    var authHeader = Request.Headers["Authorization"].ToString();
    if (string.IsNullOrEmpty(authHeader))
        return BadRequest("Missing token");

    var token = authHeader.Replace("Bearer ", "").Trim();
    if (string.IsNullOrEmpty(token))
        return BadRequest("Invalid token");

    var username = _tokenService.GetUsernameFromToken(token);
    if (string.IsNullOrEmpty(username))
        return Unauthorized("Invalid or expired token");

    var visits = await _redisService.GetTopVisitedPagesAsync(username);
    return Ok(visits);
}


}
