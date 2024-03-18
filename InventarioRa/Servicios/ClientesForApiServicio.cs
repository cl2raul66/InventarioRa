using InventarioRa.Models;
using System.Text;
using System.Text.Json;

namespace InventarioRa.Servicios;

public interface IClientesForApiServicio
{
    void Initialize(HttpClient httpClient, string serverUrl);
    Task<string> CreateAsync(Client client);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<Client>?> GetAllAsync();
    Task<Client?> GetByIdAsync(string id);
    Task<IEnumerable<string>?> GetNames();
}

public class ClientesForApiServicio : IClientesForApiServicio
{
    HttpClient? ClientHttp;
    Uri? ServerUrl;

    readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public void Initialize(HttpClient httpClient, string serverUrl)
    {
        if (ClientHttp is null)
        {
            ClientHttp = httpClient;
            ServerUrl = new Uri(serverUrl);
        }
    }

    public async Task<IEnumerable<Client>?> GetAllAsync()
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, "/Clientes"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Client>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<Client?> GetByIdAsync(string id)
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, $"/Clientes/{id}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Client>(content, jsonOptions);
        }
        return null;
    }

    public async Task<string> CreateAsync(Client client)
    {
        var json = JsonSerializer.Serialize(client);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await ClientHttp!.PostAsync(new Uri(ServerUrl!, "/Clientes"), data);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        return string.Empty;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var response = await ClientHttp!.DeleteAsync(new Uri(ServerUrl!, $"/Clientes/{id}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(content, jsonOptions);
        }
        return false;
    }

    public async Task<IEnumerable<string>?> GetNames()
    {
        var r = await GetAllAsync();
        return r?.Select(x => x.Name!);
    }
}
