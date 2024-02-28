using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using InventarioRa.Models;
using InventarioRa.Servicios;
using InventarioRa.Tools.Enums;
using InventarioRa.Tools.Messages;
using InventarioRa.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace InventarioRa.ViewModels;

public partial class PgInventarioViewModel : ObservableRecipient
{
    bool SoyYo = false;
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
        IsApiHealthy = apiServ.IsConnected;
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
                    var c = string.IsNullOrEmpty(id) ? null : await clientesServ.GetByIdAsync(id);
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
        Dictionary<string, string> clients = (await clientesServ.GetAllAsync())!.ToDictionary(x => x.Id!, x => x.Name!);
        Dictionary<string, object> sendObjects = new()
        {
            {"issale", isSale},
             {"clientes", clients},
             {"selectedinventory", si}
        };
        SoyYo = true;
        await Shell.Current.GoToAsync($"{nameof(PgInventario)}/{nameof(PgDespachoUnico)}", true, sendObjects);
    }

    [RelayCommand]
    async Task GoToDespachoVarios()
    {
        bool isSale = await Shell.Current.DisplayAlert("Tipo de despacho?", "Es venta pública", "Si", "No");
        Dictionary<string, string> clients = (await clientesServ.GetAllAsync())!.ToDictionary(x => x.Id!, x => x.Name!);
        Dictionary<string, string> inventory = Warehouse!.ToDictionary(x => x.Id!, x => x.Article!);
        Dictionary<string, object> sendObjects = new()
        {
            {"issale", isSale},
             {"clientes", clients},
             {"inventario", inventory}
        };
        SoyYo = true;
        await Shell.Current.GoToAsync($"{nameof(PgInventario)}/{nameof(PgDespachoVarios)}", true, sendObjects);
    }

    [RelayCommand]
    async Task GoToAgregar()
    {
        SoyYo = true;
        await Shell.Current.GoToAsync($"{nameof(PgInventario)}/{nameof(PgAgregarEntrada)}", true);
    }

    [RelayCommand]
    async Task Eliminar()
    {
        SoyYo = true;
        bool isOk = await Shell.Current.DisplayAlert("Precaución?", $"¿Seguro de eliminar {SelectedInventory!.Article}?", "Si", "No");
        if (isOk)
        {
            bool result = await inventarioServ.DeleteAsync(SelectedInventory.Id!);
            if (result)
            {
                Warehouse!.Remove(SelectedInventory);
                SelectedInventory = null;
            }
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
                bool result = await inventarioServ.UpdateAsync(theInventoryItem);
                if (result)
                {
                    Inventory findEle = Warehouse.First(x => x.Id == theInventoryItem.Id);
                    int idx = Warehouse.IndexOf(findEle);
                    Warehouse[idx] = theInventoryItem;
                }
            }
            else
            {
                Inventory newInventory = new() { Id = Guid.NewGuid().ToString(), Article = m.Value.Name, Existence = m.Value.Amount };
                string result = await inventarioServ.CreateAsync(newInventory);
                if (result == newInventory.Id)
                {
                    Warehouse.Insert(0, newInventory);
                }
            }
            SelectedDispatch = null;
            SelectedInventory = null;
        });

        //Para despacho de artículo único 
        WeakReferenceMessenger.Default.Register<SendDispatchChangedMessage, string>(this, "unico", async (r, m) =>
        {
            var getArticle = m.Value.Articles!.First();
            Inventory theInventoryItem = (await inventarioServ.GetByIdAsync(getArticle.Key!))!;
            theInventoryItem.Existence -= getArticle.Value;
            if (!string.IsNullOrEmpty(m.Value.ClientId) && (await clientesServ.GetByIdAsync(m.Value.ClientId!)) is null)
            {
                Client newClient = new() { Id = Guid.NewGuid().ToString(), Name = m.Value.ClientId! };
                string resultCreateCliente = await clientesServ.CreateAsync(newClient);
                if (resultCreateCliente == newClient.Id)
                {
                    m.Value.ClientId = newClient.Id;
                }
            }
            bool resultUpdate = await inventarioServ.UpdateAsync(theInventoryItem);
            if (resultUpdate)
            {
                string result = await despachosServ.CreateAsync(m.Value);
                if (string.IsNullOrEmpty(result))
                {
                    Dispatches ??= [];
                    Dispatches!.Insert(0, new()
                    {
                        DispatchId = m.Value.Id!,
                        Date = m.Value.Date.ToShortDateString(),
                        Client = string.IsNullOrEmpty(m.Value.ClientId)
                        ? "NONE"
                        : (await clientesServ.GetByIdAsync(m.Value.ClientId!))?.Name ?? "NONE",
                        Description = string.Join(", ",
                        [.. (await Task.WhenAll(m.Value.Articles!.Select(async a => $"{(await inventarioServ.GetByIdAsync(a.Key))!.Article} ({a.Value})"))),])
                    });
                }
            }
        });

        //Para despacho de artículo varios 
        WeakReferenceMessenger.Default.Register<SendDispatchChangedMessage, string>(this, "varios", async (r, m) =>
        {
            if ((await clientesServ.GetByIdAsync(m.Value.ClientId!)) is null)
            {
                Client newClient = new() { Id = Guid.NewGuid().ToString(), Name = m.Value.ClientId! };
                var resultInsertClient = await clientesServ.CreateAsync(newClient);
                if (resultInsertClient == newClient.Id)
                {
                    m.Value.ClientId = newClient.Id;
                }
            }
            string resultCreateDespacho = await despachosServ.CreateAsync(m.Value);
            if (string.IsNullOrEmpty(resultCreateDespacho))
            {
                foreach (var item in m.Value.Articles!)
                {
                    var getArticle = item;
                    Inventory theInventoryItem = (await inventarioServ.GetByIdAsync(getArticle.Key!))!;
                    theInventoryItem.Existence -= getArticle.Value;
                    _ = await inventarioServ.UpdateAsync(theInventoryItem);
                }
                Dispatches ??= [];
                Dispatches!.Insert(0, new()
                {
                    DispatchId = m.Value.Id!,
                    Date = m.Value.Date.ToShortDateString(),
                    Client = string.IsNullOrEmpty(m.Value.ClientId)
                        ? "NONE"
                        : (await clientesServ.GetByIdAsync(m.Value.ClientId!))?.Name ?? "NONE",
                    Description = string.Join(", ",
                        [.. (await Task.WhenAll(m.Value.Articles!.Select(async a => $"{(await inventarioServ.GetByIdAsync(a.Key))!.Article} ({a.Value})"))),])
                });
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
                var resultDispatch = await despachosServ.GetAllByDatesAsync((DateTime)entity.StartDate!, (DateTime)entity.EndDate!);
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
                if (SoyYo == false && IsWarehouseVisible)
                {
                    var messageObj = message.Split(':');
                    if (messageObj[1] == nameof(Inventory))
                    {
                        Warehouse ??= [];
                        Inventory Ele = (await inventarioServ.GetByIdAsync(messageObj[2]))!;
                        if (messageObj[0] == OperationType.Create.ToString())
                        {
                            Warehouse.Insert(0, Ele);
                        }
                        if (messageObj[0] == OperationType.Update.ToString())
                        {
                            var findEle = Warehouse!.FirstOrDefault(x => x.Id == messageObj[2]);
                            if (findEle is null)
                            {
                                Warehouse.Insert(0, Ele);
                            }
                            else
                            {
                                int idx = Warehouse.IndexOf(findEle);
                                Warehouse[idx] = Ele;
                            }
                        }
                        if (messageObj[0] == OperationType.Delete.ToString())
                        {
                            var findEle = Warehouse!.FirstOrDefault(x => x.Id == messageObj[2]);
                            if (Warehouse.Count > 0)
                            {
                                Warehouse.Remove(findEle!);
                            }
                        }
                    }
                }
                if (SoyYo == false && IsDispatchesVisible)
                {
                    var messageObj = message.Split(':');
                    if (messageObj[1] == nameof(Dispatch))
                    {
                        Dispatches ??= [];
                        Dispatch Ele = (await despachosServ.GetByIdAsync(messageObj[2]))!;
                        if (messageObj[0] == OperationType.Create.ToString())
                        {
                            Dispatches.Insert(0, await ToDispatchview(Ele));
                        }
                        if (messageObj[0] == OperationType.Delete.ToString())
                        {
                            if (Dispatches.Count > 0)
                            {
                                Dispatches.Remove(await ToDispatchview(Ele));
                            }
                        }
                    }
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
            getDispatches ??= await despachosServ.GetAllAsync();
            var transformTasks = getDispatches!.Select(async x => await ToDispatchview(x));
            var transform = await Task.WhenAll(transformTasks);
            Dispatches = new(transform.OrderBy(x => x.Date));
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsApiHealthy))
        {
            if (IsApiHealthy)
            {
                clientesServ.Initialize(apiServ.HttpClient, apiServ.GetServerUrl);
            }
        }

    }

    public async Task Inicializar()
    {
        if (SoyYo == false && !ItsFilteredVisisble)
        {
            await Verinventario();
        }
    }

    async Task<DispatchView> ToDispatchview(Dispatch dispatch)
    {
        return new DispatchView
        {
            DispatchId = dispatch.Id!,
            Date = dispatch.Date.ToShortDateString(),
            Client = string.IsNullOrEmpty(dispatch.ClientId) 
            ? "NONE" 
            : (await clientesServ.GetByIdAsync(dispatch.ClientId!))?.Name ?? "NONE",
            Description = string.Join(", ", [.. (await Task.WhenAll(dispatch.Articles!.Select(async a => $"{(await inventarioServ.GetByIdAsync(a.Key))!.Article} ({a.Value})")))])
        };
    }
    #endregion
}
