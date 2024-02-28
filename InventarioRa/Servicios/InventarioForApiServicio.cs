using System.Text;
using System.Text.Json;
using InventarioRa.Models;

namespace InventarioRa.Servicios;

public interface IInventarioForApiServicio
{
    void Initialize(HttpClient httpClient, string serverUrl);
    Task<string> CreateAsync(Inventory inventory);
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
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, "/Inventario/exist"));
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    public async Task<int> TotalStockAsync()
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, "/Inventario/totalstock"));
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<int>(content, jsonOptions);
    }

    public async Task<IEnumerable<Inventory>> GetAllAsync()
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, "/Inventario"));
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<Inventory>>(content, jsonOptions)!;
    }

    public async Task<Inventory?> GetByIdAsync(string id)
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, $"/Inventario/{id}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Inventory>(content, jsonOptions);
        }
        return null;
    }

    public async Task<string> CreateAsync(Inventory inventory)
    {
        var data = new StringContent(JsonSerializer.Serialize(inventory, jsonOptions), Encoding.UTF8, "application/json");
        var response = await ClientHttp!.PostAsync(new Uri(ServerUrl!, "/Inventario"), data);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        return string.Empty;
    }

    public async Task<bool> UpdateAsync(Inventory inventory)
    {
        var data = new StringContent(JsonSerializer.Serialize(inventory, jsonOptions), Encoding.UTF8, "application/json");
        var response = await ClientHttp!.PutAsync(new Uri(ServerUrl!, "/Inventario"), data);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(content, jsonOptions);
        }
        return false;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var response = await ClientHttp!.DeleteAsync(new Uri(ServerUrl!, $"/Inventario/{id}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(content, jsonOptions);
        }
        return false;
    }

    public async Task<IEnumerable<Inventory>?> GetByArticleAsync(string article)
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, $"/Inventario/article/{article}"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Inventory>>(content, jsonOptions);
        }
        return null;
    }

    public async Task<IEnumerable<string>?> GetAllArticlesAsync()
    {
        var response = await ClientHttp!.GetAsync(new Uri(ServerUrl!, "/Inventario/articles"));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<string>>(content, jsonOptions);
        }
        return null;
    }
}
