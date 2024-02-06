namespace InventarioRa.Models;

public class Dispatch
{
    public string? Id { get; set; }
    public DateTime Date { get; set; }
    public string? ClientId { get; set; } 
    public Dictionary<string, double>? Articles { get; set; } // el key es InventoryId
    public bool IsSale { get; set; }
}
