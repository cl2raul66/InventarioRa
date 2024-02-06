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
[QueryProperty(nameof(ClientsGet), "clientes")]
[QueryProperty(nameof(InventorySend), "inventario")]
public partial class PgDespachoVariosViewModel : ObservableValidator
{
    public PgDespachoVariosViewModel()
    {
        ArticlesSend ??= [];
        ArticlesSend.CollectionChanged += ArticlesSend_CollectionChanged;
    }

    private void ArticlesSend_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(this, "ArticlesSendChanged");
    }

    [ObservableProperty]
    bool isSale;

    [ObservableProperty]
    Dictionary<string, string>? clientsGet;

    [ObservableProperty]
    Dictionary<string, string>? inventorySend;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FechaShow))]
    DateTime fecha = DateTime.Now;

    public string FechaShow => Fecha.ToShortDateString();

    [ObservableProperty]
    ObservableCollection<Client>? clientes;

    [ObservableProperty]
    Client? selectedCliente; 
    
    [ObservableProperty]
    string? clienteName;

    [ObservableProperty]
    ObservableCollection<Inventory>? warehouse;

    [ObservableProperty]
    Inventory? selectedInventory;

    [ObservableProperty]
    [Required]
    ObservableCollection<ArticleDispatch>? articlesSend;

    [ObservableProperty]
    ArticleDispatch? selectedArticlesend;

    [ObservableProperty]
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

        Dispatch sendDispatch = new() { Id = Guid.NewGuid().ToString(), Date = DateTime.Now, Articles = ArticlesSend!.ToDictionary(x => x.InventoryId!, x => x.Amount), ClientId = clienteId ?? string.Empty, IsSale = IsSale };
        WeakReferenceMessenger.Default.Send(new SendDispatchChangedMessage(sendDispatch), "varios");

        SelectedCliente = null;
        ArticlesSend = null;
        ClienteName = string.Empty;
    }

    [RelayCommand]
    async Task Agregar()
    {
        _ = double.TryParse(Cantidad, out double c);

        if (SelectedInventory is null || c == 0)
        {
            VisibleErrorinfo = true;
            await Task.Delay(5000);
            VisibleErrorinfo = false;
            return;
        }

        ArticlesSend ??= [];
        var copy = new ObservableCollection<ArticleDispatch>(ArticlesSend);
        var anyArticleDispatch = copy.FirstOrDefault(x => x.InventoryId == SelectedInventory!.Id);
        if (anyArticleDispatch is not null)
        {
            anyArticleDispatch.Amount += c;
            ArticlesSend = new(copy);
        }
        else
        {
            ArticlesSend.Insert(0, new() { InventoryId = SelectedInventory!.Id, Article = SelectedInventory!.Article, Amount = c });
        }

        SelectedInventory = null;
        Cantidad = string.Empty;
    }

    [RelayCommand]
    void Eliminar()
    {
        ArticlesSend!.Remove(SelectedArticlesend!);
    }

    [RelayCommand]
    async Task GoToBack()
    {
        await Shell.Current.GoToAsync("..", true);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(ClientsGet))
        {
            Clientes = new(from x in ClientsGet! select (new Client { Id = x.Key, Name = x.Value }));
        }

        if (e.PropertyName == nameof(InventorySend))
        {
            Warehouse = new(from x in InventorySend! select (new Inventory { Id = x.Key, Article = x.Value }));
        }

        if (e.PropertyName == nameof(ArticlesSend))
        {
            
        }
    }
}
