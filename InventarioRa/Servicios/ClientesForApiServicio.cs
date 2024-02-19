using InventarioRa.Models;
using System.Text;
using System.Text.Json;

namespace InventarioRa.Servicios;

public interface IClientesForApiServicio
{
    Task<bool> ExistAsync();
    Task<bool> CreateClienteAsync(Client client);
    Task<bool> DeleteClienteAsync(string id);
    Task<IEnumerable<Client>> GetAllClientesAsync();
    Task<Client> GetClienteByIdAsync(string id);
    Task<IEnumerable<string>> GetNames();
}

public class ClientesForApiServicio : IClientesForApiServicio
{
    readonly IApiService apiServ;
    readonly Uri serverUrl;

    readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public ClientesForApiServicio(IApiService apiService)
    {
        apiServ = apiService;
        serverUrl = new Uri(apiService.GetServerUrl);
    }

    public async Task<bool> ExistAsync()
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Clientes/Exist"));
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Client>> GetAllClientesAsync()
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Clientes"));
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<Client>>(content, jsonOptions)!;
    }

    public async Task<Client> GetClienteByIdAsync(string id)
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Clientes/{id}"));
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var cliente = JsonSerializer.Deserialize<Client>(content);
        return cliente!;
    }

    public async Task<bool> CreateClienteAsync(Client client)
    {
        var json = JsonSerializer.Serialize(client);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await apiServ.HttpClient.PostAsync(new Uri(serverUrl, "/Clientes"), data);
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteClienteAsync(string id)
    {
        var response = await apiServ.HttpClient.DeleteAsync(new Uri(serverUrl, "/Clientes/{id}"));
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<string>> GetNames()
    {
        var r = await GetAllClientesAsync();
        return r.Select(x => x.Name!);
    }
}
