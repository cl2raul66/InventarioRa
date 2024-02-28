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

    public async Task JoinGroup(GroupName groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName.ToString());
        await Clients.Caller.SendAsync("JoinedGroup", groupName.ToString());
    }

    public async Task LeaveGroup(GroupName groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName.ToString());
        await Clients.Caller.SendAsync("LeftGroup", groupName.ToString());
    }
}
