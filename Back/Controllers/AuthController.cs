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

    public class PageDto
    {
        public string Path { get; set; } = "";
        public string Label { get; set; } = "";
    }

    public class PageStatDto
    {
        public string Path { get; set; } = "";
        public int Count { get; set; }
        public string Label { get; set; } = "";
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _mongoService.Users
            .Find(u => u.Username == dto.Username)
            .FirstOrDefaultAsync();

        if (existing != null)
            return BadRequest("Username already exists");

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
        var user = await _mongoService.Users
            .Find(u => u.Username == loginRequest.Username)
            .FirstOrDefaultAsync();

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            return Unauthorized("Invalid username or password");

        var jwt = _tokenService.GenerateToken(loginRequest);

        await _redisService.SetAsync(jwt, JsonSerializer.Serialize(new List<PageDto>()), TimeSpan.FromHours(2));

        string userKey = loginRequest.Username;
        bool exists = await _redisService.ExistsAsync(userKey);
        if (!exists)
            await _redisService.SetHashAsync(userKey, new Dictionary<string, int>(), TimeSpan.FromHours(12));

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

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
        if (string.IsNullOrEmpty(token)) return BadRequest("Missing token");

        var value = await _redisService.GetAsync(token);
        var pages = string.IsNullOrEmpty(value)
            ? new List<PageDto>()
            : JsonSerializer.Deserialize<List<PageDto>>(value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

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
            : JsonSerializer.Deserialize<List<PageDto>>(value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var existing = pages.FirstOrDefault(p => p.Path == dto.Path);
        if (existing != null) existing.Label = dto.Label;
        else pages.Add(new PageDto { Path = dto.Path, Label = dto.Label });

        await _redisService.SetAsync(token, JsonSerializer.Serialize(pages), TimeSpan.FromHours(2));

        return Ok();
    }

    [HttpPost("track-visit")]
    public async Task<IActionResult> TrackVisit([FromBody] PageDto dto)
    {
        try
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader)) return BadRequest("Missing token");

            var token = authHeader.Replace("Bearer ", "").Trim();
            if (string.IsNullOrEmpty(token)) return BadRequest("Invalid token");

            var username = _tokenService.GetUsernameFromToken(token);
            if (string.IsNullOrEmpty(username)) return Unauthorized("Invalid or expired token");

            if (string.IsNullOrEmpty(dto.Path)) return BadRequest("Path is required");

            await _redisService.IncrementPageVisitAsync(username, dto.Path, dto.Label ?? dto.Path);

            return Ok("Visit tracked");
        }
        catch (Exception ex)
        {
            Console.WriteLine("TrackVisit error: " + ex);
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("top-visits")]
    public async Task<IActionResult> GetTopVisits()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader)) return BadRequest("Missing token");

        var token = authHeader.Replace("Bearer ", "").Trim();
        var username = _tokenService.GetUsernameFromToken(token);
        if (string.IsNullOrEmpty(username)) return Unauthorized("Invalid or expired token");

        var visits = await _redisService.GetTopVisitedPagesAsync(username);
        return Ok(visits);
    }

    [HttpGet("global-top-pages")]
    public async Task<IActionResult> GetGlobalTopPages()
    {
        var result = await _redisService.GetGlobalTopPagesAsync(15);
        return Ok(result);
    }

    [HttpGet("global-top-dogadjaji")]
    public async Task<IActionResult> GetGlobalTopDogadjaji()
    {
        var result = await _redisService.GetGlobalTopByCategoryAsync("dogadjaj", 15);
        return Ok(result);
    }

    [HttpGet("global-top-licnosti")]
    public async Task<IActionResult> GetGlobalTopLicnosti()
    {
        var result = await _redisService.GetGlobalTopByCategoryAsync("licnost", 15);
        return Ok(result);
    }

    [HttpGet("global-top-dinastije")]
    public async Task<IActionResult> GetGlobalTopDinastije()
    {
        var result = await _redisService.GetGlobalTopByCategoryAsync("dinastija", 15);
        return Ok(result);
    }
}
