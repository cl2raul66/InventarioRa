using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using InventarioRa.Models;
using InventarioRa.Tools.Messages;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InventarioRa.ViewModels;

[QueryProperty(nameof(IsSale), "issale")]
[QueryProperty(nameof(ClientsSend), "clientes")]
[QueryProperty(nameof(SelectedInventory), "selectedinventory")]
public partial class PgDespachoUnicoViewModel : ObservableValidator
{
    [ObservableProperty]
    Dictionary<string, string>? clientsSend;

    [ObservableProperty]
    bool isSale;

    [ObservableProperty]
    ObservableCollection<Client>? clientes;

    [ObservableProperty]
    Client? selectedCliente;

    [ObservableProperty]
    string? clienteName;

    [ObservableProperty]
    Inventory? selectedInventory;

    [ObservableProperty]
    [Required]
    [MinLength(0)]
    string? cantidad;

    [ObservableProperty]
    bool visibleErrorinfo;

    [RelayCommand]
    async Task Guardar()
    {
        ValidateAllProperties();
        var existeClient = Clientes!.Any(x => x.Name!.ToUpper() == SelectedCliente?.Name?.ToUpper());
        if (HasErrors && existeClient)
        {
            VisibleErrorinfo = true;
            await Task.Delay(5000);
            VisibleErrorinfo = false;
            return;
        }

        string? clienteId = string.IsNullOrEmpty(SelectedCliente?.Id) ? ClienteName : SelectedCliente.Id;

        Dispatch sendDispatch = new() { Id = Guid.NewGuid().ToString(), Date = DateTime.Now, Articles = new() { { selectedInventory!.Id!, double.Parse(Cantidad!)! } }, ClientId = clienteId ?? string.Empty, IsSale = IsSale };
        _ = WeakReferenceMessenger.Default.Send(new SendDispatchChangedMessage(sendDispatch), "unico");

        SelectedCliente = null;
        Cantidad = string.Empty;
        ClienteName = string.Empty;
    }

    [RelayCommand]
    async Task GoToBack()
    {
        await Shell.Current.GoToAsync("..", true);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(ClientsSend))
        {
            Clientes = new(from x in ClientsSend! select new Client { Id = x.Key, Name = x.Value });
        }
    }
}
