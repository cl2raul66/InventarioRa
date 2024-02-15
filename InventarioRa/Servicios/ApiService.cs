using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace InventarioRa.Servicios;

public interface IApiService
{
    string GetServerUrl { get; }
    HttpClient HttpClient { get; }

    event Action<string, string>? OnNotificationReceived;
    bool IsConnected { get; }

    Task ConnectAsync();
    Task<bool> SetUrl([Url] string url);
}

public class ApiService : IApiService
{
    readonly string? key;
    HubConnection? _connection;
    public HttpClient HttpClient { get; private set; }

    public ApiService(IConfiguration configuration)
    {
        key = configuration.GetSection("Settings")["Key"];
        HttpClient = new HttpClient
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
                var response = await HttpClient.GetAsync(new Uri(new Uri(url), "/healthchecks").ToString());
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine("El servidor está saludable.");
                        Preferences.Default.Set(key!, new Uri(new Uri(url), "/serverStatusHub").ToString());
                        return true;
                    case HttpStatusCode.ServiceUnavailable:
                        Console.WriteLine("El servidor no está saludable.");
                        break;
                    default:
                        Console.WriteLine($"Código de estado desconocido: {response.StatusCode}");
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

    public event Action<string, string>? OnNotificationReceived;

    public async Task ConnectAsync()
    {
        if (_connection is null)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(GetServerUrl)
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string>("ReceiveStatusMessage", async (message) =>
            {
                if (message == "El servidor va a detenerse")
                {
                    await Reconnect();
                }
            });

            _connection.Reconnected += async (connectionId) =>
            {
                await Task.CompletedTask;
            };

            _connection.Closed += async (error) =>
            {
                if (_connection.State is HubConnectionState.Disconnected)
                {
                    await Reconnect();
                }
            };
        }

        if (_connection.State is HubConnectionState.Disconnected)
        {
            await _connection.StartAsync();
            // Notificar al usuario
            OnNotificationReceived?.Invoke("Conexión", "Cliente conectado.");
        }
    }

    private async Task Reconnect()
    {
        if (_connection is null)
        {
            return;
        }

        while (true)
        {
            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                    break;
                }
            }
            catch (Exception ex)
            {
                await Task.Delay(3000);
            }
        }
    }

    public bool IsConnected => _connection?.State is HubConnectionState.Connected;
}
