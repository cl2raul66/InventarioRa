using InventarioRa.Models;
using System.Text;
using System.Text.Json;

namespace InventarioRa.Servicios;

public interface IClientesForApiServicio
{
    Task<bool> ExistAsync();
    Task<bool> CreateClienteAsync(Client client);
    Task<bool> DeleteClienteAsync(string id);
    Task<IEnumerable<Client>?> GetAllClientesAsync();
    Task<Client?> GetClienteByIdAsync(string id);
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
        ClientHttp = httpClient;
        ServerUrl = new Uri(serverUrl);
    }

    public async Task<bool> ExistAsync()
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, "/Clientes/Exist"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(content, jsonOptions);
        }
        return false;
    }

    public async Task<IEnumerable<Client>?> GetAllClientesAsync()
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, "/Clientes"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Client>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<Client?> GetClienteByIdAsync(string id)
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, $"/Clientes/{id}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Client>(content, jsonOptions);
        }
        return null;
    }

    public async Task<bool> CreateClienteAsync(Client client)
    {
        var json = JsonSerializer.Serialize(client);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await ClientHttp!.PostAsync(new Uri(ServerUrl!, "/Clientes"), data);
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteClienteAsync(string id)
    {
        var response = await ClientHttp!.DeleteAsync(new Uri(ServerUrl!, $"/Clientes/{id}"));
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<string>?> GetNames()
    {
        var r = await GetAllClientesAsync();
        return r?.Select(x => x.Name!);
    }
}
