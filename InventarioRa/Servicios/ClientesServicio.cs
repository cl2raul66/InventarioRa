﻿using InventarioRa.Models;
using LiteDB;

namespace InventarioRa.Servicios;

public class ClientesServicio
{
    readonly ILiteCollection<Client> collection;

    public ClientesServicio()
    {
        var cnx = new ConnectionString()
        {
            Filename = Path.Combine(FileSystem.Current.AppDataDirectory, "Clientes.db")
        };

        LiteDatabase db = new(cnx);
        collection = db.GetCollection<Client>();
    }

    public bool Exist => collection.Count() > 0;

    public IEnumerable<Client> GetAll() => collection.FindAll().Reverse();

    public Client? GetById(string id) => collection.FindById(id);

    public bool Insert(Client client) => collection.Insert(client) is not null;

    public bool Delete(string id) => collection.Delete(id);


    public IEnumerable<string> GetNames() => collection.FindAll().Select(x => x.Name).Reverse()!;
    public string? GetId(string? name) => collection.FindOne(x => x.Name == name)?.Id;
}
