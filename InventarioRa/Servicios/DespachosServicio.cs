using InventarioRa.Models;
using LiteDB;

namespace InventarioRa.Servicios;

public interface IDespachosServicio
{
    bool Exist { get; }

    bool Delete(string id);
    IEnumerable<Dispatch> GetAll();
    public IEnumerable<string> GetAllInventoryId();
    IEnumerable<Dispatch> GetAllByClientId(string? clientId);
    IEnumerable<Dispatch> GetAllByInventoryId(string inventoryId);
    IEnumerable<Dispatch> GetAllByDate(DateTime startDate, DateTime endDate);
    Dispatch? GetById(string id);
    bool Insert(Dispatch inventory);
}

public class DespachosServicio : IDespachosServicio
{
    readonly ILiteCollection<Dispatch> collection;

    public DespachosServicio()
    {
        var cnx = new ConnectionString()
        {
            Filename = Path.Combine(FileSystem.Current.AppDataDirectory, "Despachos.db")
        };

        LiteDatabase db = new(cnx);
        collection = db.GetCollection<Dispatch>();
    }

    public bool Exist => collection.Count() > 0;

    public IEnumerable<Dispatch> GetAll() => collection.FindAll().Reverse();

    public Dispatch? GetById(string id) => collection.FindById(id);

    public bool Insert(Dispatch inventory) => collection.Insert(inventory) is not null;

    public bool Delete(string id) => collection.Delete(id);

    public IEnumerable<Dispatch> GetAllByClientId(string? clientId) => collection.Find(x => x.ClientId == clientId);
    public IEnumerable<Dispatch> GetAllByInventoryId(string inventoryId) => collection.FindAll().Where(x => x.Articles != null && x.Articles!.ContainsKey(inventoryId));
    public IEnumerable<Dispatch> GetAllByDate(DateTime startDate, DateTime endDate) => collection.Find(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date);
    public IEnumerable<string> GetAllInventoryId() => collection.FindAll().SelectMany(d => d.Articles!.Keys).Distinct();
}
