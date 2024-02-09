using Microsoft.AspNetCore.SignalR.Client;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace InventarioRa.Servicios;

public interface IApiClientService
{
    string GetServerUrl { get; }

    HttpClient Current();
    Task<bool> SetUrl([Url] string url);
    Task<bool> Test([Url] string? url = null);
}

public class ApiClientService : IApiClientService
{
    readonly string key = "D86B1695D10443D0979C5F39DE7801A2";
    readonly HttpClient httpClient;

    public ApiClientService()
    {
        httpClient = new();
    }

    public async Task<bool> SetUrl([Url] string url)
    {
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            return false;
        }
        bool hasResponse = await Test(url);

        if (hasResponse)
        {
            Preferences.Default.Set(key, url);
            return !string.IsNullOrEmpty(Preferences.Default.Get(key, string.Empty));
        }
        return hasResponse;
    }

    public string GetServerUrl
    {
        get
        {
            return Preferences.Default.Get(key, string.Empty);
        }
    }

    public HttpClient Current() => httpClient;

    public async Task<bool> Test([Url] string? url = null)
    {
        if (string.IsNullOrEmpty(url))
        {
            url = Preferences.Default.Get(key, string.Empty);
        }
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            return false;
        }

        HubConnection connection = null;
        try
        {
            // Crear una nueva conexión al hub de SignalR
            connection = new HubConnectionBuilder()
                .WithUrl($"{url}/serverStatusHub")
                .WithAutomaticReconnect()
                .Build();

            // Iniciar la conexión
            await connection.StartAsync();

            // Si la conexión se inició correctamente, entonces el servidor está en línea
            return connection.State == HubConnectionState.Connected;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"No se pudo conectar al servidor: {ex.Message}");
            return false;
        }
        finally
        {
            // Cerrar la conexión
            if (connection != null)
            {
                await connection.StopAsync();
                await connection.DisposeAsync();
            }
        }
    }

}
