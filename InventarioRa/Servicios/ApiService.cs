using Microsoft.AspNetCore.SignalR.Client;
using System.ComponentModel.DataAnnotations;

namespace InventarioRa.Servicios;

public interface IApiService
{
    string GetServerUrl { get; }
    HttpClient HttpClient { get; }

    event Action<string, string>? OnNotificationReceived;

    Task ConnectAsync();
    void SetUrl([Url] string url);
    Task<bool> TestConnection(string? url = null);
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

    public void SetUrl([Url] string url)
    {
        if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            Preferences.Default.Set(key, url);
        }
    }

    public string GetServerUrl => Preferences.Default.Get(key, string.Empty);

    public async Task<bool> TestConnection(string? url = null)
    {
        if (string.IsNullOrEmpty(url))
        {
            url = GetServerUrl;
        }

        HubConnection? connection = null;
        try
        {
            // Crear una nueva conexión al hub de SignalR
            connection = new HubConnectionBuilder()
                .WithUrl($"{url}/serverStatusHub")
                .WithAutomaticReconnect()
                .Build();

            // Crear un CancellationToken con un tiempo de espera de 5 segundos
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Iniciar la conexión
            await connection.StartAsync(cts.Token);

            // Si la conexión se inició correctamente, entonces el servidor está en línea
            var r = connection.State is HubConnectionState.Connected;
            return r;
        }
        catch (OperationCanceledException)
        {
            // La operación fue cancelada porque se agotó el tiempo de espera
            Console.WriteLine("No se pudo conectar al servidor: se agotó el tiempo de espera.");
            return false;
        }
        catch (Exception ex)
        {
            var r = ex is null;
            var t = ex.GetType;
            var m = ex.Message;
            var s = ex.Source;
            var d = ex.Data;
            Console.WriteLine($"No se pudo conectar al servidor: {ex.Message}");
            return false;
        }
        finally
        {
            // Cerrar la conexión
            if (connection is not null)
            {
                await connection.StopAsync();
                await connection.DisposeAsync();
            }
        }
    }

    public event Action<string, string>? OnNotificationReceived;

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
                OnNotificationReceived?.Invoke("ReceiveMessage", message);
            });

            _connection.On<string>("ReceiveStatusMessage", (message) =>
            {
                OnNotificationReceived?.Invoke("ReceiveStatusMessage", message);
            });

            await _connection.StartAsync();
        }
    }
}