using InventarioRaServer.Models;
using InventarioRaServer.Tools;
using LiteDB;

namespace InventarioRaServer.Services;

public interface IInventarioServicio
{
    bool Exist { get; }
    int TotalStock { get; }

    bool Delete(string id);
    IEnumerable<Inventory> GetAll();
    IEnumerable<string> GetAllArticle();
    Inventory? GetById(string id);
    IEnumerable<Inventory>? GetByArticle(string article);
    bool Insert(Inventory inventory);
    bool Update(Inventory inventory);
}

public class InventarioServicio : IInventarioServicio
{
    readonly ILiteCollection<Inventory> collection;

    public InventarioServicio()
    {
        var cnx = new ConnectionString()
        {
            Filename = Helpers.GetFileDbPath("Invertario")
        };

        LiteDatabase db = new(cnx);
        collection = db.GetCollection<Inventory>();
    }

    public bool Exist => collection.Count() > 0;

    public int TotalStock => collection.Count();

    public IEnumerable<Inventory> GetAll() => collection.FindAll().Reverse();

    public Inventory? GetById(string id) => collection.FindById(id);

    public bool Insert(Inventory inventory) => collection.Insert(inventory) is not null;

    public bool Update(Inventory inventory) => collection.Update(inventory);

    public bool Delete(string id) => collection.Delete(id);


    public IEnumerable<Inventory>? GetByArticle(string article) => collection.Find(x => x.Article!.Contains(article));
    public IEnumerable<string> GetAllArticle() => collection.FindAll().Select(x => x.Article!);
}
