using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using InventarioRa.Models;
using InventarioRa.Servicios;
using InventarioRa.Tools.Messages;
using InventarioRa.Views;
using System.Collections.ObjectModel;

namespace InventarioRa.ViewModels;

public partial class PgInventarioViewModel : ObservableRecipient
{
    readonly IInventarioServicio inventarioServ;
    readonly IClientesServicio clientesServ;
    readonly IDespachosServicio despachosServ;

    public PgInventarioViewModel(IInventarioServicio inventarioServicio, IClientesServicio clientesServicio, IDespachosServicio despachosServicio)
    {
        IsActive = true;
        inventarioServ = inventarioServicio;
        clientesServ = clientesServicio;
        despachosServ = despachosServicio;
        Verinventario();
    }

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
    async Task VerFiltrar()
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
                var articles = (from e in despachosServ.GetAllInventoryId() select inventarioServ.GetById(e)).Select(x=>new ArticleInventory { InventoryId = x.Id, Article = x.Article }).ToList();
                await Shell.Current.GoToAsync(nameof(PgBuscarDespachos), true, new Dictionary<string, object> { { "articles", articles } });
            }
        }
        else
        {
            if (IsWarehouseVisible)
            {
                GetWarehouse();
            }
            if (IsDispatchesVisible)
            {
                GetDispatch();
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
        Dictionary<string, string> clients = clientesServ.GetAll().ToDictionary(x => x.Id!, x => x.Name!);
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
        Dictionary<string, string> clients = clientesServ.GetAll().ToDictionary(x => x.Id!, x => x.Name!);
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
            bool resul = inventarioServ.Delete(SelectedInventory.Id!);
            if (resul)
            {
                Warehouse!.Remove(SelectedInventory);
            }
        }
    }

    [RelayCommand]
    void Verinventario()
    {
        ItsFilteredVisisble = false;
        SelectedDispatch = null;
        GetWarehouse();
        IsDispatchesVisible = false;
        IsWarehouseVisible = true;
        Dispatches = null;
    }

    [RelayCommand]
    void Verdespachachos()
    {
        ItsFilteredVisisble = false;
        SelectedInventory = null;
        GetDispatch();
        IsWarehouseVisible = false;
        IsDispatchesVisible = true;
        Warehouse = null;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        //Para entrada de artículos
        WeakReferenceMessenger.Default.Register<PgInventarioViewModel, SendArticleentryChangedMessage>(this, (r, m) =>
        {
            Warehouse ??= [];
            Inventory? existingInventoryItem = Warehouse.FirstOrDefault(x => x.Article == m.Value.Name);
            if (existingInventoryItem is not null)
            {
                Inventory theInventoryItem = inventarioServ.GetById(existingInventoryItem.Id!)!;
                theInventoryItem.Existence += m.Value.Amount;
                bool result = inventarioServ.Update(theInventoryItem);
                if (result)
                {
                    GetWarehouse();
                }
            }
            else
            {
                Inventory newInventory = new() { Id = Guid.NewGuid().ToString(), Article = m.Value.Name, Existence = m.Value.Amount };
                bool result = inventarioServ.Insert(newInventory);
                if (result)
                {
                    GetWarehouse();
                }
            }
        });

        //Para despacho de artículo único 
        WeakReferenceMessenger.Default.Register<SendDispatchChangedMessage, string>(this, "unico", (r, m) =>
        {
            var getArticle = m.Value.Articles!.First();
            Inventory theInventoryItem = inventarioServ.GetById(getArticle.Key!)!;
            theInventoryItem.Existence -= getArticle.Value;
            if (clientesServ.GetById(m.Value.ClientId!) is null)
            {
                Client newClient = new() { Id = Guid.NewGuid().ToString(), Name = m.Value.ClientId! };
                var result = clientesServ.Insert(newClient);
                if (result)
                {
                    m.Value.ClientId = newClient.Id;
                }
            }
            bool resultUpdate = inventarioServ.Update(theInventoryItem);
            if (resultUpdate)
            {
                bool result = despachosServ.Insert(m.Value);
                if (result)
                {
                    GetWarehouse();
                }
            }
        });

        //Para despacho de artículo varios 
        WeakReferenceMessenger.Default.Register<SendDispatchChangedMessage, string>(this, "varios", (r, m) =>
        {
            if (clientesServ.GetById(m.Value.ClientId!) is null)
            {
                Client newClient = new() { Id = Guid.NewGuid().ToString(), Name = m.Value.ClientId! };
                var resultInsertClient = clientesServ.Insert(newClient);
                if (resultInsertClient)
                {
                    m.Value.ClientId = newClient.Id;
                }
            }
            bool result = despachosServ.Insert(m.Value);
            if (result)
            {
                bool resultUpdate = false;
                foreach (var item in m.Value.Articles!)
                {
                    var getArticle = item;
                    Inventory theInventoryItem = inventarioServ.GetById(getArticle.Key!)!;
                    theInventoryItem.Existence -= getArticle.Value;
                    resultUpdate = inventarioServ.Update(theInventoryItem);
                }
                if (resultUpdate)
                {
                    GetWarehouse();
                }
            }
        });

        //Para buscar en Inventario
        WeakReferenceMessenger.Default.Register<SearchInventoryForSearchChangedMessage>(this, (r, m) =>
        {
            SelectedInventory = null;

            var result = inventarioServ.GetByArticle(m.Value);
            Warehouse = result is not null ? new(result) : null;
        });

        //Para buscar en Despacho
        WeakReferenceMessenger.Default.Register<SendDispatchForSearchChangedMessage>(this, (r, m) =>
        {
            SelectedDispatch = null;
            DispatchForSearch entity = m.Value;
            //buscar por fecha
            bool isStartDate = entity.StartDate is not null;
            if (isStartDate)
            {
                entity.EndDate ??= DateTime.Now;
                var resultDispatch = despachosServ.GetAllByDate((DateTime)entity.StartDate!, (DateTime)entity.EndDate!);
                if (resultDispatch is not null)
                {
                    GetDispatch(resultDispatch!);
                }
                else
                {
                    Dispatches = null;
                }
            }
            else if (entity.Article is not null) //buscar por articulo
            {
                var resultDispatch = despachosServ.GetAllByInventoryId(entity.Article.InventoryId!);
                if (resultDispatch is not null)
                {
                    GetDispatch(resultDispatch!);
                }
                else
                {
                    Dispatches = null;
                }
            }
            else //buscar por cliente
            {
                string? resultClientId = clientesServ.GetId(entity.Client);

                var resultDispatch = despachosServ.GetAllByClientId(resultClientId);
                if (resultDispatch is not null)
                {
                    GetDispatch(resultDispatch!);
                }
                else
                {
                    Dispatches = null;
                }
            }
        });
    }

    #region Extra
    void GetWarehouse()
    {
        if (inventarioServ.Exist)
        {
            Warehouse = new(inventarioServ.GetAll().OrderBy(x => x.Article));
        }
    }

    void GetDispatch(IEnumerable<Dispatch>? getDispatches = null)
    {
        if (despachosServ.Exist)
        {
            getDispatches ??= despachosServ.GetAll();
            Dispatches = new(getDispatches!.Select(x => new DispatchView { DispatchId = x.Id!, Date = x.Date.ToShortDateString(), Client = string.IsNullOrEmpty(x.ClientId) ? "NONE" : clientesServ.GetById(x.ClientId!)?.Name ?? "NONE", Description = string.Join(", ", x.Articles!.Select(x => $"{inventarioServ.GetById(x.Key)!.Article} ({x.Value})")) }).OrderBy(x => x.Date));
        }
    }
    #endregion
}
