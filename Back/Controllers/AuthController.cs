using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;

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

        //var role = dto.CustomClaims?.Role ?? "user";

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
        if (loginRequest == null)
            return BadRequest("Invalid login request");

            var user = await _mongoService.Users.Find(u => u.Username == loginRequest.Username).FirstOrDefaultAsync();

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            return Unauthorized("Invalid username or password");

        var jwt = _tokenService.GenerateToken(loginRequest);

        await _redisService.SetAsync(jwt, user.Id, TimeSpan.FromHours(2));

        return Ok(new
        {
            token = jwt,
            expiresIn = _tokenService.TokenLifetime.TotalSeconds
        });
    }

    //nije napravljeno 
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader)) return BadRequest("Missing token");

        var token = authHeader.Replace("Bearer ", "");
        await _redisService.DeleteAsync(token);

        return Ok("Logged out");
    }
}
