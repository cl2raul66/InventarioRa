using Microsoft.AspNetCore.SignalR;

namespace InventarioRaServer.Tools;

public class NotificationHub : Hub
{
    public static ServerStatus Status = ServerStatus.Running;

    public async Task SendStatusMessage(string statusMessage)
    {
        if (Enum.TryParse(statusMessage, out ServerStatus status))
        {
            Status = status;
            await Clients.All.SendAsync("ReceiveStatusMessage", statusMessage);
        }
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ReceiveStatusMessage", Status.ToString());
        await base.OnConnectedAsync();
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("JoinedGroup", groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("LeftGroup", groupName);
    }
}
