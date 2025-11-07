using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class SecretController : ControllerBase
{
    [HttpGet("any")]
    [Authorize]
    public IActionResult Any()
    {
        var username = User.Identity?.Name;
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        return Ok($"Hello {username} ({role}) - any authenticated user");
    }

    [HttpGet("getadminrole")]
    [Authorize(Roles = "admin")]
    public IActionResult AdminRole()
    {
        var username = User.Identity?.Name;
        return Ok($"Hello {username}, only admins can see this!");
    }

    [HttpGet("user")]
    [Authorize(Roles = "user")]
    public IActionResult UserSecret()
    {
        var username = User.Identity?.Name;
        return Ok($"Hello {username}, only users can see this!");
    }

    [HttpGet("admin")]
    [Authorize(Policy = IdentityData.AdminPolicyName)]
    public IActionResult AdminPolicy()
    {
        var username = User.Identity?.Name;
        return Ok($"Hello {username}, only admins can see this!");
    }


}

