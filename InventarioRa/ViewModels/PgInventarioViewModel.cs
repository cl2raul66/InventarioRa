using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using InventarioRa.Models;
using InventarioRa.Servicios;
using InventarioRa.Tools.Messages;
using InventarioRa.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace InventarioRa.ViewModels;

public partial class PgInventarioViewModel : ObservableRecipient
{
    readonly IApiService apiServ;
    readonly IInventarioForApiServicio inventarioServ;
    readonly IClientesForApiServicio clientesServ;
    readonly IDespachosForApiServicio despachosServ;

    public PgInventarioViewModel(IApiService apiService, IClientesForApiServicio clientesServicio, IInventarioForApiServicio inventarioServicio, IDespachosForApiServicio despachosServicio)
    {
        IsActive = true;
        apiServ = apiService;
        inventarioServ = inventarioServicio;
        clientesServ = clientesServicio;
        despachosServ = despachosServicio;
        apiServ.OnNotificationsReceived += ApiServ_OnNotificationReceived;
    }

    [ObservableProperty]
    bool isApiHealthy;

    [ObservableProperty]
    bool isWarehouseVisible;

    [ObservableProperty]
    ObservableCollection<Inventory>? warehouse;

    [ObservableProperty]
    Inventory? selectedInventory;

    [ObservableProperty]
    bool isDispatchesVisible;

    [ObservableProperty]
    ObservableCollection<DispatchView>? dispatches;

    [ObservableProperty]
    DispatchView? selectedDispatch;

    [ObservableProperty]
    bool itsFilteredVisisble;

    [RelayCommand]
    public async Task VerFiltrar()
    {
        ItsFilteredVisisble = !ItsFilteredVisisble;
        if (ItsFilteredVisisble)
        {
            if (IsWarehouseVisible)
            {
                await Shell.Current.GoToAsync(nameof(PgBuscarInventario), true);
            }
            if (IsDispatchesVisible)
            {
                var inventoryIds = await despachosServ.GetAllInventoryIdsAsync();
                var articles = await Task.WhenAll(inventoryIds!.Select(async id =>
                {
                    var inventory = await inventarioServ.GetByIdAsync(id);
                    return new ArticleInventory { InventoryId = inventory!.Id, Article = inventory.Article };
                }));
                var clientIds = await despachosServ.GetAllClientIdsAsync();
                var clientsSend = await Task.WhenAll(clientIds!.Select(async id =>
                {
                    var c = string.IsNullOrEmpty(id) ? null : await clientesServ.GetClienteByIdAsync(id);
                    return c ?? new Client() { Id = id, Name = "NONE" };
                }));
                await Shell.Current.GoToAsync(nameof(PgBuscarDespachos), true, new Dictionary<string, object> {
                    { "articles", articles },
                    { "clients", clientsSend }
                });
            }
        }
        else
        {
            if (IsWarehouseVisible)
            {
                await GetWarehouse();
            }
            if (IsDispatchesVisible)
            {
                await GetDispatch();
            }
        }
    }


    [RelayCommand]
    async Task GoToBack()
    {
        await Shell.Current.GoToAsync("..", true);
    }

    [RelayCommand]
    async Task GoToDespachoUnico()
    {
        var si = new Inventory { Id = SelectedInventory!.Id, Article = SelectedInventory!.Article, Existence = SelectedInventory!.Existence };
        SelectedInventory = null;
        bool isSale = await Shell.Current.DisplayAlert("Tipo de despacho?", "Es venta pública", "Si", "No");
        Dictionary<string, string> clients = (await clientesServ.GetAllClientesAsync())!.ToDictionary(x => x.Id!, x => x.Name!);
        Dictionary<string, object> sendObjects = new()
        {
            {"issale", isSale},
             {"clientes", clients},
             {"selectedinventory", si}
        };
        await Shell.Current.GoToAsync($"{nameof(PgInventario)}/{nameof(PgDespachoUnico)}", true, sendObjects);
    }

    [RelayCommand]
    async Task GoToDespachoVarios()
    {
        bool isSale = await Shell.Current.DisplayAlert("Tipo de despacho?", "Es venta pública", "Si", "No");
        Dictionary<string, string> clients = (await clientesServ.GetAllClientesAsync())!.ToDictionary(x => x.Id!, x => x.Name!);
        Dictionary<string, string> inventory = Warehouse!.ToDictionary(x => x.Id!, x => x.Article!);
        Dictionary<string, object> sendObjects = new()
        {
            {"issale", isSale},
             {"clientes", clients},
             {"inventario", inventory}
        };
        await Shell.Current.GoToAsync($"{nameof(PgInventario)}/{nameof(PgDespachoVarios)}", true, sendObjects);
    }

    [RelayCommand]
    async Task GoToAgregar()
    {
        await Shell.Current.GoToAsync($"{nameof(PgInventario)}/{nameof(PgAgregarEntrada)}", true);
    }

    [RelayCommand]
    async Task Eliminar()
    {
        bool isOk = await Shell.Current.DisplayAlert("Precaución?", $"¿Seguro de eliminar {SelectedInventory!.Article}?", "Si", "No");
        if (isOk)
        {
            _ = await inventarioServ.DeleteAsync(SelectedInventory.Id!);
            //if (resul)
            //{
            //    Warehouse!.Remove(SelectedInventory);
            //}
        }
    }

    [RelayCommand]
    async Task Verinventario()
    {
        ItsFilteredVisisble = false;
        SelectedDispatch = null;
        await GetWarehouse();
        IsDispatchesVisible = false;
        IsWarehouseVisible = true;
        Dispatches = null;
    }

    [RelayCommand]
    async Task Verdespachachos()
    {
        ItsFilteredVisisble = false;
        SelectedInventory = null;
        await GetDispatch();
        IsWarehouseVisible = false;
        IsDispatchesVisible = true;
        Warehouse = null;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        //Para entrada de artículos
        WeakReferenceMessenger.Default.Register<PgInventarioViewModel, SendArticleentryChangedMessage>(this, async (r, m) =>
        {
            Warehouse ??= [];
            Inventory? existingInventoryItem = Warehouse.FirstOrDefault(x => x.Article == m.Value.Name);
            if (existingInventoryItem is not null)
            {
                Inventory theInventoryItem = (await inventarioServ.GetByIdAsync(existingInventoryItem.Id!))!;
                theInventoryItem!.Existence += m.Value.Amount;
                _ = await inventarioServ.UpdateAsync(theInventoryItem);
            }
            else
            {
                Inventory newInventory = new() { Id = Guid.NewGuid().ToString(), Article = m.Value.Name, Existence = m.Value.Amount };
                _ = await inventarioServ.CreateAsync(newInventory);
            }
        });

        //Para despacho de artículo único 
        WeakReferenceMessenger.Default.Register<SendDispatchChangedMessage, string>(this, "unico", async (r, m) =>
        {
            var getArticle = m.Value.Articles!.First();
            Inventory theInventoryItem = (await inventarioServ.GetByIdAsync(getArticle.Key!))!;
            theInventoryItem.Existence -= getArticle.Value;
            if (!string.IsNullOrEmpty(m.Value.ClientId) && (await clientesServ.GetClienteByIdAsync(m.Value.ClientId!)) is null)
            {
                Client newClient = new() { Id = Guid.NewGuid().ToString(), Name = m.Value.ClientId! };
                var result = await clientesServ.CreateClienteAsync(newClient);
                if (result)
                {
                    m.Value.ClientId = newClient.Id;
                }
            }
            bool resultUpdate = await inventarioServ.UpdateAsync(theInventoryItem);
            if (resultUpdate)
            {
                _ = await despachosServ.CreateDespachoAsync(m.Value);
            }
        });

        //Para despacho de artículo varios 
        WeakReferenceMessenger.Default.Register<SendDispatchChangedMessage, string>(this, "varios", async (r, m) =>
        {
            if ((await clientesServ.GetClienteByIdAsync(m.Value.ClientId!)) is null)
            {
                Client newClient = new() { Id = Guid.NewGuid().ToString(), Name = m.Value.ClientId! };
                var resultInsertClient = await clientesServ.CreateClienteAsync(newClient);
                if (resultInsertClient)
                {
                    m.Value.ClientId = newClient.Id;
                }
            }
            bool result = await despachosServ.CreateDespachoAsync(m.Value);
            if (result)
            {
                //bool resultUpdate = false;
                foreach (var item in m.Value.Articles!)
                {
                    var getArticle = item;
                    Inventory theInventoryItem = (await inventarioServ.GetByIdAsync(getArticle.Key!))!;
                    theInventoryItem.Existence -= getArticle.Value;
                    _ = await inventarioServ.UpdateAsync(theInventoryItem);
                }
            }
        });

        //Para buscar en Inventario
        WeakReferenceMessenger.Default.Register<SearchInventoryForSearchChangedMessage>(this, async (r, m) =>
        {
            SelectedInventory = null;
            var result = await inventarioServ.GetByArticleAsync(m.Value);
            Warehouse = result is not null ? new(result) : null;
        });

        //Para buscar en Despacho
        WeakReferenceMessenger.Default.Register<SendDispatchForSearchChangedMessage>(this, async (r, m) =>
        {
            SelectedDispatch = null;
            DispatchForSearch entity = m.Value;
            //buscar por fecha
            bool isStartDate = entity.StartDate is not null;
            if (isStartDate)
            {
                entity.EndDate ??= DateTime.Now;
                var resultDispatch = await despachosServ.GetAllByDateAsync((DateTime)entity.StartDate!, (DateTime)entity.EndDate!);
                if (resultDispatch is not null)
                {
                    await GetDispatch(resultDispatch!);
                }
                else
                {
                    Dispatches = null;
                }
            }
            else if (entity.Article is not null) //buscar por articulo
            {
                var resultDispatch = await despachosServ.GetAllByInventoryIdAsync(entity.Article.InventoryId!);
                if (resultDispatch is not null)
                {
                    await GetDispatch(resultDispatch!);
                }
                else
                {
                    Dispatches = null;
                }
            }
            else //buscar por cliente
            {
                var resultDispatch = await despachosServ.GetAllByClientIdAsync(entity.Client);
                if (resultDispatch is not null)
                {
                    await GetDispatch(resultDispatch!);
                }
                else
                {
                    Dispatches = null;
                }
            }
        });
    }

    #region Extra
    async void ApiServ_OnNotificationReceived(string channel, string message)
    {
        switch (channel)
        {
            case "ReceiveMessage":
                if (IsWarehouseVisible)
                {
                    if (
                    message.Contains("Un nuevo inventario ha sido agregado"))
                    {
                        
                    }
                    if (message.Contains("Un inventario ha sido actualizado"))
                    {

                    }
                    if (message.Contains("Un inventario ha sido eliminado"))
                    {

                    }
                }
                {
                    await GetWarehouse();
                }
                if (IsDispatchesVisible && (
                    message.Contains("Un nuevo despacho ha sido agregado")
                    || message.Contains("Un despacho ha sido eliminado")))
                {
                    await GetDispatch();
                }
                break;
            case "ReceiveStatusMessage":
                IsApiHealthy = message == "El servidor está iniciando";
                break;
        }
    }
    async Task GetWarehouse()
    {
        if (await inventarioServ.ExistAsync())
        {
            Warehouse = new((await inventarioServ.GetAllAsync()).OrderBy(x => x.Article));
        }
    }

    async Task GetDispatch(IEnumerable<Dispatch>? getDispatches = null)
    {
        if (await despachosServ.ExistAsync())
        {
            getDispatches ??= await despachosServ.GetAllDespachosAsync();
            var transformTasks = getDispatches!.Select(async x => new DispatchView
            {
                DispatchId = x.Id!,
                Date = x.Date.ToShortDateString(),
                Client = string.IsNullOrEmpty(x.ClientId)
                    ? "NONE"
                    : (await clientesServ.GetClienteByIdAsync(x.ClientId!))?.Name ?? "NONE",
                Description = string.Join(", ",
                [
                    .. (await Task.WhenAll(x.Articles!.Select(async a => $"{(await inventarioServ.GetByIdAsync(a.Key))!.Article} ({a.Value})"))),
                ])
            });
            var transform = await Task.WhenAll(transformTasks);
            Dispatches = new(transform.OrderBy(x => x.Date));
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Warehouse))
        {
            var r = Warehouse?.Count ?? 0;
        }
    }

    public async void Inicializar()
    {
        if (!ItsFilteredVisisble)
        {
            await Verinventario();
        }
    }
    #endregion
}
