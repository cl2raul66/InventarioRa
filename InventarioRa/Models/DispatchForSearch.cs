namespace InventarioRa.Models;

public class DispatchForSearch
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Client { get; set; }
    public ArticleInventory? Article { get; set; }
}
