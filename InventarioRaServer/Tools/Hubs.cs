using Microsoft.AspNetCore.SignalR;

namespace InventarioRaServer.Tools;

public class NotificationHub : Hub
{
    public static string ServerStatus = "El servidor está iniciando";

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ReceiveStatusMessage", ServerStatus);
        await base.OnConnectedAsync();
    }

    public async Task SendStatusMessage(string message)
    {
        ServerStatus = message;
        await Clients.All.SendAsync("ReceiveStatusMessage", message);
    }
}
