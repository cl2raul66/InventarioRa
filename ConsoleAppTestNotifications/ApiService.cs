using Microsoft.AspNetCore.SignalR.Client;

namespace ConsoleAppTestNotifications;

public class ApiService
{
    private HubConnection connection;

    public event Action<string> OnStatusMessageReceived;
    public event Action<string> OnMessageReceived;

    public ApiService(string url)
    {
        connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();

        connection.On<string>("ReceiveStatusMessage", (message) =>
        {
            OnStatusMessageReceived?.Invoke(message);
        });

        connection.On<string>("ReceiveMessage", (message) =>
        {
            OnMessageReceived?.Invoke(message);
        });

        connection.Closed += async (error) =>
        {
            if (connection.State is HubConnectionState.Disconnected)
            {
                await Task.Run(Reconnect);
            }
        };
    }

    public async void Reconnect()
    {
        while (true)
        {
            try
            {
                if (connection.State == HubConnectionState.Disconnected)
                {
                    await connection.StartAsync();
                    break;
                }
            }
            catch (Exception ex)
            {
                await Task.Delay(3000);
            }
        }
    }

    public async void Connect()
    {
        while (true)
        {
            try
            {
                await connection.StartAsync();
                break;
            }
            catch (Exception ex)
            {
                await Task.Delay(3000);
            }
        }
    }
}
