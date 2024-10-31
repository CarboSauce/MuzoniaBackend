using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Muzonia.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloWorld(
    ILogger<HelloWorld> logger,
    IConnectionMultiplexer redis
) : ControllerBase
{
    [HttpGet("{key}/{value}")]
    public async Task<IActionResult> Create(string key, string value)
    {
        logger.LogInformation("Creating key: {key}", key);
        var db = redis.GetDatabase();
        await db.StringSetAsync(key, value);
        return Ok();
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Read(string key)
    {
        logger.LogInformation("Reading key: {key}", key);
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (value.HasValue)
        {
            return Ok(value.ToString());
        }
        else
        {
            return BadRequest();
        }
    }
}
