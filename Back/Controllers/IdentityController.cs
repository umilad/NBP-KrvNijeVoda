using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

public class IdentityController : ControllerBase
{
    private readonly MongoService _mongoService;
    private readonly RedisService _redisService;
    private const string TokenSecret = "this_is_my_super_secret_key_12345";
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(2);
    public IdentityController(MongoService mongoService, RedisService redisService)
    {
        _mongoService = mongoService;
        _redisService = redisService;
    }

    //[HttpPost("token")]
    
    private string GenerateToken(LoginDto loginRequest)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, loginRequest.Username),
            new(JwtRegisteredClaimNames.Name, loginRequest.Username),
            new(IdentityData.RoleClaimName, loginRequest.CustomClaims.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(TokenLifetime),
            Issuer = "http://localhost:5210",
            Audience = "http://localhost:5210",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token); // returns string
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginDto dto)
    {
        var existing = await _mongoService.Users.Find(u => u.Username == dto.Username).FirstOrDefaultAsync();
        if (existing != null) return BadRequest("Username already exists");

        //var role = dto.CustomClaims?.Role ?? "user";

        var user = new UserMongo
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            //UserId = dto.UserId,
            Role = dto.CustomClaims.Role
            //Role = "User" // default role       
        };

        await _mongoService.Users.InsertOneAsync(user);
        return Ok("User registered");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginRequest)
    {
        if (loginRequest == null)
            return BadRequest("Invalid login request");

            // 1️⃣ Validate user exists in MongoDB
            var user = await _mongoService.Users.Find(u => u.Username == loginRequest.Username).FirstOrDefaultAsync();

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            return Unauthorized("Invalid username or password");

        // 2️⃣ Generate JWT using your existing method
        var jwt = GenerateToken(loginRequest);

        // 3️⃣ Optionally: store token in Redis for multi-device sessions
        await _redisService.SetAsync(jwt, user.Id, TimeSpan.FromHours(2));

        // 4️⃣ Return the token
        return Ok(new
        {
            token = jwt,
            expiresIn = TokenLifetime.TotalSeconds
        });
    }

    
}