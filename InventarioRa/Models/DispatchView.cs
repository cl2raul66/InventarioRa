using CommunityToolkit.Mvvm.ComponentModel;
using InventarioRa.Servicios;

namespace InventarioRa.Models;

public class DispatchView
{
    public string DispatchId { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Client { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}