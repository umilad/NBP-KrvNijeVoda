using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
public class RedisTestController : ControllerBase
{
    private readonly RedisService _redis;

    public RedisTestController(RedisService redis)
    {
        _redis = redis;
    }

    [HttpGet("runtest")]
    public async Task<IActionResult> RunTest()
    {
        await _redis.RunTestAsync();
        var value = await _redis.GetAsync("foo");
        return Ok(new { value });
    }
}
