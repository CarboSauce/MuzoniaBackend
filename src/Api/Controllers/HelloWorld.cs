using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Muzonia.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloWorld(ILogger<HelloWorld> logger) : ControllerBase
{
    [HttpGet("{key}/{value}")]
    public Task<IActionResult> Create(string key, string value)
    {
        logger.LogInformation("Creating key: {key}", key);
        return Task.FromResult<IActionResult>(Ok(new { key, value }));
    }

    [HttpGet("{key}")]
    public Task<IActionResult> Read(string key)
    {
        logger.LogInformation("Reading key: {key}", key);

        return Task.FromResult<IActionResult>(Ok(new { hello = key }));
    }
}
