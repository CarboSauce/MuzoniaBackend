using Microsoft.AspNetCore.SignalR;

namespace Muzonia.Api.Hubs;

public class PlayerHub(ILogger<PlayerHub> logger) : Hub
{
    public async Task Ping()
    {
        logger.LogInformation("Pinging");
        await Clients.All.SendAsync("Pong");
    }
}
