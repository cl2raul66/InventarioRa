namespace InventarioRaServer.Models;

public class Client
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}

public class Dispatch
{
    public string? Id { get; set; }
    public DateTime Date { get; set; }
    public string? ClientId { get; set; }
    public Dictionary<string, double>? Articles { get; set; } // el key es InventoryId
    public bool IsSale { get; set; }
}

public class Inventory
{
    public string? Id { get; set; }
    public string? Article { get; set; }
    public double Existence { get; set; }
}
