using InventarioRa.Models;
using System.Text;
using System.Text.Json;

namespace InventarioRa.Servicios;

public interface IDespachosForApiServicio
{
    Task<bool> CreateDespachoAsync(Dispatch dispatch);
    Task<bool> DeleteDespachoAsync(string id);
    Task<bool> ExistAsync();
    Task<IEnumerable<Dispatch>?> GetAllByClientIdAsync(string? clientId);
    Task<IEnumerable<Dispatch>?> GetAllByDateAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Dispatch>?> GetAllByInventoryIdAsync(string inventoryId);
    Task<IEnumerable<Dispatch>?> GetAllDespachosAsync();
    Task<IEnumerable<string>?> GetAllInventoryIdsAsync();
    Task<IEnumerable<string>?> GetAllClientIdsAsync();
    Task<Dispatch?> GetDespachoByIdAsync(string id);
}

public class DespachosForApiServicio : IDespachosForApiServicio
{
    readonly IApiService apiServ;
    readonly Uri serverUrl;

    readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public DespachosForApiServicio(IApiService apiService)
    {
        apiServ = apiService;
        serverUrl = new Uri(apiService.GetServerUrl);
    }

    public async Task<IEnumerable<Dispatch>?> GetAllDespachosAsync()
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Despachos"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Dispatch>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<Dispatch?> GetDespachoByIdAsync(string id)
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, $"/Despachos/{id}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Dispatch>(content, jsonOptions);
        }
        return null;
    }

    public async Task<bool> CreateDespachoAsync(Dispatch dispatch)
    {
        var content = new StringContent(JsonSerializer.Serialize(dispatch, jsonOptions), Encoding.UTF8, "application/json");
        var response = await apiServ.HttpClient.PostAsync(new Uri(serverUrl, "/Despachos"), content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteDespachoAsync(string id)
    {
        var response = await apiServ.HttpClient.DeleteAsync(new Uri(serverUrl, $"/Despachos/{id}"));
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Dispatch>?> GetAllByClientIdAsync(string? clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            return await GetAllByClientIdNullAsync();
        }
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, $"/Despachos/byClientId/{clientId}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Dispatch>>(content, jsonOptions);
        }
        return null;
    }
    
    async Task<IEnumerable<Dispatch>?> GetAllByClientIdNullAsync()
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Despachos/byClientIdNull"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Dispatch>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<IEnumerable<Dispatch>?> GetAllByInventoryIdAsync(string inventoryId)
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, $"/Despachos/byInventoryId/{inventoryId}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Dispatch>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<IEnumerable<Dispatch>?> GetAllByDateAsync(DateTime startDate, DateTime endDate)
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, $"/Despachos/byDate?startDate={startDate:s}&endDate={endDate:s}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Dispatch>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<IEnumerable<string>?> GetAllInventoryIdsAsync()
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Despachos/allInventoryIds"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<string>>(content, jsonOptions);
        }
        return null;
    }
    
    public async Task<IEnumerable<string>?> GetAllClientIdsAsync()
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Despachos/allClientIds"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<string>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<bool> ExistAsync()
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Despachos/exist"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(content, jsonOptions);
        }
        return false;
    }
}
