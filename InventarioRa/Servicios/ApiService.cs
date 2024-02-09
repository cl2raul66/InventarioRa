using Microsoft.AspNetCore.SignalR.Client;
using System.ComponentModel.DataAnnotations;

namespace InventarioRa.Servicios;

public interface IApiService
{
    string GetServerUrl { get; }
    HttpClient HttpClient { get; }
    Task SetUrl([Url] string url);
    bool TestConnection();
    event Action<string>? OnNotificationReceived;
    Task ConnectAsync();
}

public class ApiService : IApiService
{
    readonly string key = "D86B1695D10443D0979C5F39DE7801A2";
    public HttpClient HttpClient { get; private set; }
    HubConnection? _connection;

    public ApiService()
    {
        HttpClient = new HttpClient();
    }

    public Task SetUrl([Url] string url)
    {
        if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            Preferences.Default.Set(key, url);
        }
        return Task.CompletedTask;
    }

    public string GetServerUrl => Preferences.Default.Get(key, string.Empty);

    public bool TestConnection()
    {
        return _connection?.State == HubConnectionState.Connected;
    }

    public event Action<string>? OnNotificationReceived;

    public async Task ConnectAsync()
    {
        if (!string.IsNullOrEmpty(GetServerUrl))
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(GetServerUrl)
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string>("ReceiveMessage", (message) =>
            {
                OnNotificationReceived?.Invoke(message);
            });

            _connection.On<string>("ReceiveStatusMessage", (message) =>
            {
                OnNotificationReceived?.Invoke(message);
            });

            await _connection.StartAsync();
        }
    }
}