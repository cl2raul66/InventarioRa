using InventarioRa.Tools.Enums;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace InventarioRa.Servicios;

public interface IApiService
{
    string GetServerUrl { get; }
    HttpClient HttpClient { get; }

    event Action<string, string>? OnNotificationsReceived;
    bool IsConnected { get; }

    Task ConnectAsync();
    Task<bool> SetUrl([Url] string url);
}

public class ApiService : IApiService
{
    readonly string? key;
    HubConnection? connection;
    Uri? serverUrl;

    public event Action<string, string>? OnNotificationsReceived;

    public HttpClient HttpClient { get; private set; }

    public ApiService(IConfiguration configuration)
    {
        key = configuration.GetSection("Settings")["Key"];
        HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<bool> SetUrl([Url] string url)
    {
        if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            try
            {
                serverUrl = new Uri(url);
                var response = await HttpClient.GetAsync(new Uri(serverUrl, "/healthchecks").ToString());
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        Preferences.Default.Set(key!, serverUrl.ToString());
                        return true;
                    default:
                        Console.WriteLine($"Error al intentar acceder a la URL: {response.StatusCode}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al intentar acceder a la URL: {ex.Message}");
            }
        }

        return false;
    }

    public string GetServerUrl => Preferences.Default.Get(key!, string.Empty);

    public async Task ConnectAsync()
    {
        connection = new HubConnectionBuilder()
            .WithUrl(new Uri(new Uri(GetServerUrl), "/serverStatusHub"))
            .WithAutomaticReconnect()
            .Build();

        connection.On<string>("ReceiveStatusMessage", (message) =>
        {
            OnNotificationsReceived?.Invoke("ReceiveStatusMessage", message);
        });

        connection.On<string>("ReceiveMessage", (message) =>
        {
            OnNotificationsReceived?.Invoke("ReceiveMessage", message);
        });

        connection.Closed += async (error) =>
        {
            if (connection.State is HubConnectionState.Disconnected)
            {
                await Task.Run(Reconnect);
            }
        };

        await Reconnect();
    }

    public async Task JoinGroup(GroupName groupName)
    {
        if (connection is not null)
        {
            await connection.InvokeAsync("JoinGroup", groupName.ToString());
        }
    }

    public async Task LeaveGroup(GroupName groupName)
    {
        if (connection is not null)
        {
            await connection.InvokeAsync("LeaveGroup", groupName.ToString());
        }
    }

    public bool IsConnected => connection?.State is HubConnectionState.Connected;

    #region Extra
    private async Task Reconnect()
    {
        if (connection is null)
        {
            return;
        }

        while (true)
        {
            try
            {
                if (connection.State is HubConnectionState.Disconnected)
                {
                    await connection.StartAsync();
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await Task.Delay(3000);
            }
        }
    }
    #endregion
}
