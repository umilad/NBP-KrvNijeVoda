using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenService
{
    private readonly IConfiguration _config;
    private const string TokenSecret = "this_is_my_super_secret_key_12345";
    public readonly TimeSpan TokenLifetime = TimeSpan.FromHours(2);

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(LoginDto loginRequest)
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
        return tokenHandler.WriteToken(token);
    }
    
    public string? GetUsernameFromToken(string token)
{
    var handler = new JwtSecurityTokenHandler();

    if (!handler.CanReadToken(token))
        return null;

    var jwtToken = handler.ReadJwtToken(token);

    // tvoj token sadrži i "sub" i "name", koristi bilo koji koji postoji
    var usernameClaim = jwtToken.Claims.FirstOrDefault(c =>
        c.Type == JwtRegisteredClaimNames.Sub ||
        c.Type == JwtRegisteredClaimNames.Name ||
        c.Type == "unique_name");

    return usernameClaim?.Value;
}

}
