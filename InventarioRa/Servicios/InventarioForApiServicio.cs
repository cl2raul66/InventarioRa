using System.Text;
using System.Text.Json;
using InventarioRa.Models;

namespace InventarioRa.Servicios;

public interface IInventarioForApiServicio
{
    Task<bool> CreateAsync(Inventory inventory);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistAsync();
    Task<IEnumerable<string>?> GetAllArticlesAsync();
    Task<IEnumerable<Inventory>> GetAllAsync();
    Task<IEnumerable<Inventory>?> GetByArticleAsync(string article);
    Task<Inventory?> GetByIdAsync(string id);
    Task<int> TotalStockAsync();
    Task<bool> UpdateAsync(Inventory inventory);
}

public class InventarioForApiServicio : IInventarioForApiServicio
{
    readonly IApiService apiServ;
    readonly Uri serverUrl;

    readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public InventarioForApiServicio(IApiService apiService)
    {
        apiServ = apiService;
        serverUrl = new Uri(apiService.GetServerUrl);
        
    }

    public async Task<bool> ExistAsync()
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Inventario/exist"));
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    public async Task<int> TotalStockAsync()
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Inventario/totalstock"));
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<int>(content, jsonOptions);
    }

    public async Task<IEnumerable<Inventory>> GetAllAsync()
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Inventario"));
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<Inventory>>(content, jsonOptions)!;
    }

    public async Task<Inventory?> GetByIdAsync(string id)
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, $"/Inventario/{id}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Inventory>(content, jsonOptions);
        }
        return null;
    }

    public async Task<bool> CreateAsync(Inventory inventory)
    {
        var content = new StringContent(JsonSerializer.Serialize(inventory, jsonOptions), Encoding.UTF8, "application/json");
        var response = await apiServ.HttpClient.PostAsync(new Uri(serverUrl, "/Inventario"), content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(Inventory inventory)
    {
        var content = new StringContent(JsonSerializer.Serialize(inventory, jsonOptions), Encoding.UTF8, "application/json");
        var response = await apiServ.HttpClient.PutAsync(new Uri(serverUrl, "/Inventario"), content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var response = await apiServ.HttpClient.DeleteAsync(new Uri(serverUrl, $"/Inventario/{id}"));
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Inventory>?> GetByArticleAsync(string article)
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, $"/Inventario/article/{article}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Inventory>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<IEnumerable<string>?> GetAllArticlesAsync()
    {
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Inventario/articles"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<string>>(content, jsonOptions);
        }
        return null;
    }
}
