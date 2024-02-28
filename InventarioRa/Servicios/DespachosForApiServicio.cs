using InventarioRa.Models;
using System.Text;
using System.Text.Json;

namespace InventarioRa.Servicios;

public interface IDespachosForApiServicio
{
    void Initialize(HttpClient httpClient, string serverUrl);
    Task<string> CreateAsync(Dispatch dispatch);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistAsync();
    Task<IEnumerable<Dispatch>?> GetAllByClientIdAsync(string? clientId);
    Task<IEnumerable<Dispatch>?> GetAllByDatesAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Dispatch>?> GetAllByInventoryIdAsync(string inventoryId);
    Task<IEnumerable<Dispatch>?> GetAllAsync();
    Task<IEnumerable<string>?> GetAllInventoryIdsAsync();
    Task<IEnumerable<string>?> GetAllClientIdsAsync();
    Task<Dispatch?> GetByIdAsync(string id);
}

public class DespachosForApiServicio : IDespachosForApiServicio
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

    public async Task<IEnumerable<Dispatch>?> GetAllAsync()
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, "/Despachos"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Dispatch>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<Dispatch?> GetByIdAsync(string id)
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, $"/Despachos/{id}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Dispatch>(content, jsonOptions);
        }
        return null;
    }

    public async Task<string> CreateAsync(Dispatch dispatch)
    {
        var data = new StringContent(JsonSerializer.Serialize(dispatch, jsonOptions), Encoding.UTF8, "application/json");
        var response = await ClientHttp!.PostAsync(new Uri(ServerUrl!, "/Despachos"), data);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        return string.Empty;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var response = await ClientHttp!.DeleteAsync(new Uri(ServerUrl!, $"/Despachos/{id}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(content, jsonOptions);
        }
        return false;
    }

    public async Task<IEnumerable<Dispatch>?> GetAllByClientIdAsync(string? clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            return await GetAllByClientIdNullAsync();
        }
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, $"/Despachos/byClientId/{clientId}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Dispatch>>(content, jsonOptions);
        }
        return null;
    }
    
    async Task<IEnumerable<Dispatch>?> GetAllByClientIdNullAsync()
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, "/Despachos/byClientIdNull"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Dispatch>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<IEnumerable<Dispatch>?> GetAllByInventoryIdAsync(string inventoryId)
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, $"/Despachos/byInventoryId/{inventoryId}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Dispatch>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<IEnumerable<Dispatch>?> GetAllByDatesAsync(DateTime startDate, DateTime endDate)
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, $"/Despachos/byDate?startDate={startDate:s}&endDate={endDate:s}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Dispatch>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<IEnumerable<string>?> GetAllInventoryIdsAsync()
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, "/Despachos/allInventoryIds"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<string>>(content, jsonOptions);
        }
        return null;
    }
    
    public async Task<IEnumerable<string>?> GetAllClientIdsAsync()
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, "/Despachos/allClientIds"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<string>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<bool> ExistAsync()
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, "/Despachos/exist"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(content, jsonOptions);
        }
        return false;
    }
}
