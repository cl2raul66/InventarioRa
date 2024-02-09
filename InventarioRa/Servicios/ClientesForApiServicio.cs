using InventarioRa.Models;
using System.Text;
using System.Text.Json;

namespace InventarioRa.Servicios;

public interface IClientesForApiServicio
{
    Task<bool> ExistAsync();
    Task<bool> CreateClienteAsync(Client client);
    Task DeleteClienteAsync(string id);
    Task<IEnumerable<Client>> GetAllClientesAsync();
    Task<Client> GetClienteByIdAsync(string id);
    Task<IEnumerable<string>> GetNames();
}

public class ClientesForApiServicio : IClientesForApiServicio
{
    readonly HttpClient _httpClient;
    readonly string serverUrl;
    readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ClientesForApiServicio(IApiClientService apiClientService)
    {
        _httpClient = apiClientService.Current();
        serverUrl = apiClientService.GetServerUrl;
    }

    public async Task<bool> ExistAsync()
    {
        var response = await _httpClient!.GetAsync($"{serverUrl}/Clientes/Exist");
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Client>> GetAllClientesAsync()
    {
        var response = await _httpClient!.GetAsync($"{serverUrl}/Clientes");
        var content = await response.Content.ReadAsStringAsync();
        var clientes = JsonSerializer.Deserialize<IEnumerable<Client>>(content);
        return clientes!;
    }

    public async Task<Client> GetClienteByIdAsync(string id)
    {
        var response = await _httpClient!.GetAsync($"{serverUrl}/Clientes/{id}");
        var content = await response.Content.ReadAsStringAsync();
        var cliente = JsonSerializer.Deserialize<Client>(content);
        return cliente!;
    }

    public async Task<bool> CreateClienteAsync(Client client)
    {
        var json = JsonSerializer.Serialize(client);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient!.PostAsync($"{serverUrl}/Clientes", data);
        return response.IsSuccessStatusCode;
    }

    public async Task DeleteClienteAsync(string id)
    {
        await _httpClient!.DeleteAsync($"{serverUrl}/Clientes/{id}");
    }

    public async Task<IEnumerable<string>> GetNames()
    {
        var r = await GetAllClientesAsync();
        return r.Select(x => x.Name!);
    }
}
