using InventarioRa.Models;
using LiteDB;

namespace InventarioRa.Servicios;

public class InventarioServicio
{
    readonly ILiteCollection<Inventory> collection;

    public InventarioServicio()
    {
        var cnx = new ConnectionString()
        {
            Filename = Path.Combine(FileSystem.Current.AppDataDirectory, "Invertario.db")
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
