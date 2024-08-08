using Microsoft.AspNetCore.Mvc;

namespace Muzonia.Core.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloWorld(ILogger<HelloWorld> logger) : ControllerBase
{
    [HttpGet("{text}")]
    public async Task<IActionResult> Greet(string text)
    {
        logger.LogInformation("Greeted with {text}", text);
        await Task.Delay(1000);
        return Ok($"Hello, {text}");
    }
}
