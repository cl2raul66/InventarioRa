using Microsoft.AspNetCore.SignalR.Client;

namespace InventarioRa.Servicios;

public interface INotificationApiService
{
    event Action<string>? OnNotificationReceived;

    Task ConnectAsync();
}

public class NotificationApiService : INotificationApiService
{
    readonly HubConnection? _connection;
    readonly string serverUrl;

    public NotificationApiService(IApiClientService apiClientService)
    {
        serverUrl = apiClientService.GetServerUrl;
        if (string.IsNullOrEmpty(serverUrl))
        {
            return;
        }
        _connection = new HubConnectionBuilder()
            .WithUrl(serverUrl)
            .WithAutomaticReconnect()
            .Build();
    }

    public event Action<string>? OnNotificationReceived;

    public async Task ConnectAsync()
    {
        _connection!.On<string>("ReceiveMessage", (message) =>
        {
            OnNotificationReceived?.Invoke(message);
        });

        _connection!.On<string>("ReceiveStatusMessage", (message) =>
        {
            OnNotificationReceived?.Invoke(message);
        });

        await _connection!.StartAsync();
    }
}
