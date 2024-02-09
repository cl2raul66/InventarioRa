using Microsoft.AspNetCore.SignalR;

namespace InventarioRaServer.Tools;

public class NotificationHub : Hub
{
    public async Task SendStatusMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveStatusMessage", message);
    }
}
