using InventarioRaServer.Models;
using InventarioRaServer.Tools;
using LiteDB;

namespace InventarioRaServer.Services;

public interface IClientesServicio
{
    bool Delete(string id);
    IEnumerable<Client> GetAll();
    Client? GetById(string id);
    IEnumerable<string> GetNames();
    string? GetId(string? name);
    string Insert(Client client);
}

public class ClientesServicio : IClientesServicio
{
    readonly ILiteCollection<Client> collection;

    public ClientesServicio()
    {
        var cnx = new ConnectionString()
        {
            Filename = Helpers.GetFileDbPath("Clientes")
        };

        LiteDatabase db = new(cnx);
        collection = db.GetCollection<Client>();

        if (collection.Count() == 0)
        {
            Insert(new Client() { Id = ObjectId.NewObjectId().ToString() });
        }
    }

    public IEnumerable<Client> GetAll() => collection.FindAll().Reverse();

    public Client? GetById(string id) => collection.FindById(id);

    public string Insert(Client client)
    {
        try
        {
            return collection.Insert(client).AsString;
        }
        catch (Exception)
        {

            return string.Empty;
        }
    }

    public bool Delete(string id) => collection.Delete(id);


    public IEnumerable<string> GetNames() => collection.FindAll().Select(x => x.Name).Reverse()!;
    public string? GetId(string? name) => collection.FindOne(x => x.Name == name)?.Id;
}
