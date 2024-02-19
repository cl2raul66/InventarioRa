using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InventarioRa.Servicios;

public class InventarioForApiServicio
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
        var response = await apiServ.HttpClient.GetAsync(new Uri(serverUrl, "/Inventario/Exist"));
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    public int TotalStock => collection.Count();

    public IEnumerable<Inventory> GetAll() => collection.FindAll().Reverse();

    public Inventory? GetById(string id) => collection.FindById(id);

    public bool Insert(Inventory inventory) => collection.Insert(inventory) is not null;

    public bool Update(Inventory inventory) => collection.Update(inventory);

    public bool Delete(string id) => collection.Delete(id);


    public IEnumerable<Inventory>? GetByArticle(string article) => collection.Find(x => x.Article!.Contains(article));
    public IEnumerable<string> GetAllArticle() => collection.FindAll().Select(x => x.Article!);
}
