using System.Text.Json;

public class RequestTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RedisService _redisService;

    public RequestTrackingMiddleware(RequestDelegate next, RedisService redisService)
    {
        _next = next;
        _redisService = redisService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Dohvati postojeću istoriju ili kreiraj novu listu
            var value = await _redisService.GetAsync(token);
            var pages = string.IsNullOrEmpty(value)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(value) ?? new List<string>();

            var path = context.Request.Path.Value ?? "";

            if (!string.IsNullOrWhiteSpace(path) &&
                !path.Contains("login") &&
                !path.Contains("logout") &&
                !path.Contains("history") &&
                !pages.Contains(path))
            {
                pages.Add(path);
                var updatedJson = JsonSerializer.Serialize(pages);
                await _redisService.SetAsync(token, updatedJson, TimeSpan.FromHours(2));
            }
        }

        await _next(context);
    }
}
