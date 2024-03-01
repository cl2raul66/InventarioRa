using InventarioRaServer.Models;
using InventarioRaServer.Tools;
using LiteDB;

namespace InventarioRaServer.Services;

public interface IDespachosServicio
{
    bool Exist { get; }

    bool Delete(string id);
    IEnumerable<Dispatch> GetAll();
    IEnumerable<string> GetAllInventoryId();
    IEnumerable<string> GetAllClientIds();
    IEnumerable<Dispatch> GetAllByClientId(string? clientId);
    IEnumerable<Dispatch> GetAllByInventoryId(string inventoryId);
    IEnumerable<Dispatch> GetAllByDate(DateTime startDate, DateTime endDate);
    Dispatch? GetById(string id);
    string Insert(Dispatch inventory);
}

public class DespachosServicio : IDespachosServicio
{
    readonly ILiteCollection<Dispatch> collection;

    public DespachosServicio()
    {
        var cnx = new ConnectionString()
        {
            Filename = Helpers.GetFileDbPath("Despachos")
        };

        LiteDatabase db = new(cnx);
        collection = db.GetCollection<Dispatch>();
    }

    public bool Exist => collection.Count() > 0;

    public IEnumerable<Dispatch> GetAll() => collection.FindAll().Reverse();

    public Dispatch? GetById(string id) => collection.FindById(id);

    public string Insert(Dispatch dispatch)
    {
        try
        {
            return collection.Insert(dispatch).AsString;
        }
        catch (Exception)
        {

            return string.Empty;
        }
    }

    public bool Delete(string id) => collection.Delete(id);

    public IEnumerable<Dispatch> GetAllByClientId(string? clientId) => collection.Find(x => x.ClientId == clientId);
    public IEnumerable<Dispatch> GetAllByInventoryId(string inventoryId) => collection.FindAll().Where(x => x.Articles != null && x.Articles!.ContainsKey(inventoryId));
    public IEnumerable<Dispatch> GetAllByDate(DateTime startDate, DateTime endDate) => collection.Find(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date);
    public IEnumerable<string> GetAllInventoryId() => collection.FindAll().SelectMany(d => d.Articles!.Keys).Distinct();
    public IEnumerable<string> GetAllClientIds() => collection.FindAll().Select(d => d.ClientId ?? string.Empty).Distinct();
}
